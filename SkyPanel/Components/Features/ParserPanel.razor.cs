using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using SkyPanel.Components.Dialogs;
using SkyPanel.Components.Services;
using SkyPanel.Utils;

namespace SkyPanel.Components.Features;

public partial class ParserPanel : ComponentBase
{
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
    
    private Task OpenFileDialogAsync()
    {
        var parameters = new DialogParameters<FileDialogParser>
        {
            { x => x.ParserName, ParserState.ParserName },
            
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        return DialogService.ShowAsync<FileDialogParser>("Upload Dataset", parameters, options);
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
            }
            
        }
        
    }
    
    [CascadingParameter]
    private Task<AuthenticationState> authenticationStateTask { get; set; }
    
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
}