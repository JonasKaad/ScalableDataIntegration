using System.Net.Http.Headers;
using CommonDis.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SkyPanel.Components.Dialogs;
using SkyPanel.Components.Models;
using SkyPanel.Components.Services;
using SkyPanel.Utils;

namespace SkyPanel.Components.Features;

public partial class ParserPanel : ComponentBase
{
    private readonly ILogger<ParserPanel> _logger;
    public ParserPanel (ILogger<ParserPanel> logger)
    {
        _logger = logger;
    }

    [CascadingParameter]
    private Task<AuthenticationState> authenticationStateTask { get; set; }
    [Inject] private ParserStateService ParserState { get; set; } = null!;
    [Inject] private BlobManagerService BlobService { get; set; } = null!;
    
    [Inject] private OrchestratorClientService OrchestratorClient { get; set; } = null!;
    
    private string _selectedParser = string.Empty;
    
    public string Parser
    {
        get => string.IsNullOrEmpty(ParserState.ParserName) ? _selectedParser : ParserState.ParserName;
        set
        {
            _selectedParser = value;
            _ = InvokeAsync(async () => 
            {
                await GetDownloaderConfiguration(value);
                StateHasChanged();
            });
        }
    }
    private async Task GetDownloaderConfiguration(string parserName)
    {
        if (string.IsNullOrEmpty(parserName))
        {
            ParserState.SetParser(null);
            return;
        }
        
        // Get the full parser configuration and store it
        var parserConfig = await OrchestratorClient.GetDownloaderConfiguration(parserName);
        ParserState.SetParser(parserConfig);
        StateHasChanged();
    }
    
    protected override void OnInitialized()
    {
        ParserState.OnChange += StateHasChanged;
    }
    
    private async Task OpenFileDialogAsync()
    {
        var parameters = new DialogParameters<FileDialogParser>
        {
            { x => x.ParserName, ParserState.ParserName },
            
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        var dialogResult = await (await DialogService.ShowAsync<FileDialogParser>("Upload dataset", parameters, options)).Result;
        
        var result = dialogResult?.Data as IList<IBrowserFile> ?? [];
        
        if (result.Count > 0)
        {
            await Upload(result, ParserState.ParserName);
        }
    }

    private async Task OpenReparseDialogAsync()
    {
        var parameters = new DialogParameters<ReparseDialog>
        {
            { x => x.ContentText, "Do you want to fetch and parse the latest dataset for:" },
            { x => x.ConfirmationButtonText, "Confirm" },
            { x => x.EmphasizedCenterText, ParserState.ParserName },
        };
        
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall, FullWidth = true };
        var dialogResult = await (await DialogService.ShowAsync<ReparseDialog>("Fetch and parse latest dataset", parameters, options)).Result;
        var result = dialogResult?.Data as string ?? string.Empty;
        
        if (result == "reparse")
        {
            var reparseResult = await OrchestratorClient.Reparse(ParserState.ParserName);
            if (reparseResult)
            {
                Snackbar.Add($"Started fetching and parsing latest dataset for {ParserState.ParserName}",  Severity.Info);
            }
            else
            {
                Snackbar.Add($"Failed to fetch and parse latest dataset for {ParserState.ParserName}", Severity.Error);
                _logger.LogError("[AUDIT] Failed to fetch and parse latest dataset for {Parser}", ParserState.ParserName);
            }
        }
    }

    private async Task OpenCreateDialogAsync()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true};
        var dialogResult = await (await DialogService.ShowAsync<CreateParserDialog>("Create new Downloader", options)).Result;

        if(dialogResult?.Data is DownloaderData data)
        {
            var downloaderName = data.Name;
            var result = await OrchestratorClient.CreateDownloader(data);
            if (result)
            {
                Snackbar.Add($"Created new downloader {downloaderName}", Severity.Success);
                _logger.LogInformation("[AUDIT] Created new downloader {Downloader}", downloaderName);
            }
            else
            {
                Snackbar.Add($"Failed to create downloader {downloaderName}", Severity.Error);
                _logger.LogError("[AUDIT] Failed to create downloader {Downloader}", downloaderName);
            }
        }
    }


    private List<UploadResult> uploadResults = new();
    private List<File> _files = new();
    
    private async Task Upload(IList<IBrowserFile> files, string parserName)
    {
        // 30 MB
        long maxFileSize = 30 * 1000000;
        var upload = false;
        _files.Clear();
        
        using var content = new MultipartFormDataContent();

        foreach (var file in files)
        {
            try
            {
                _files.Add(new() { Name = file.Name });

                var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));
                    fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(file.ContentType);

                content.Add(
                    content: fileContent,
                    name: "\"formFiles\"",
                    fileName: file.Name);

                upload = true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(
                    "{FileName} not uploaded: {Message}",
                    file.Name, ex.Message);
                Snackbar.Add($"Failed to upload {file.Name} \n{ex.Message}", Severity.Error);
            }
            
        }
        if (upload)
        {
            var response = await OrchestratorClient.UploadFiles(parserName, content);

            if (response.Success)
            {
                var authState = await authenticationStateTask;
                var authUser = authState.User;
                var user = RoleUtil.GetUserEmail(authUser);
                var fileNames = string.Join(", ", _files.Select(x => x.Name));
                _logger.LogInformation( "[AUDIT] {User} uploaded: {files} to {Parser}", user, fileNames, parserName);
                Snackbar.Add($"Uploaded {_files.Count} files!", Severity.Success);
            }
            else if (response.Result == Result.AlreadyExists)
            {
                Snackbar.Add($"Blob with same timestamp already exists! \n\nWait a minute and try again.", Severity.Warning);
            }
            else if (response.Result == Result.FileFormatError)
            {
                Snackbar.Add($"Wrong file format! Check the content of the file and try again.", Severity.Error);
            }
            else
            {
                Snackbar.Add($"Failed to upload files! \n{response.Result} \n{response.Message}", Severity.Error);
                var fileNames = string.Join(", ", _files.Select(x => x.Name));
                _logger.LogInformation("{FileName} not uploaded {reason}", fileNames, response.Result);
            }
        }
        
    }
    
    
    private async Task<IEnumerable<string>> Search(string value, CancellationToken token)
    {
        var downloaders = await OrchestratorClient.GetDownloaders();
        var authenticationState = await authenticationStateTask;
        var user = authenticationState.User;

        if (user.IsInRole("Admin"))
        {
            if (string.IsNullOrEmpty(value))
            {
                return downloaders;
            }
            return downloaders.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }

        var downloadersToReturn = new List<string>();
        foreach (var downloader in downloaders)
        {
            if (RoleUtil.HasRole(downloader, user))
            {
                downloadersToReturn.Add(downloader);
            }
        }
        if (string.IsNullOrEmpty(value))
        {
            return downloadersToReturn;
        }
        return downloadersToReturn.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }

    private class File
    {
        public string? Name { get; set; }
    }
}