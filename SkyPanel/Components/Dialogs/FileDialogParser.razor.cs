using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SkyPanel.Utils;
using SkyPanel.Components.Models;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Dialogs;

public partial class FileDialogParser : ComponentBase
{
    private readonly ILogger<FileDialogParser> _logger;
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    private void DialogSubmit(IList<IBrowserFile> files)
    {
        MudDialog?.Close(DialogResult.Ok(files));
    }

    private void DialogCancel() => MudDialog?.Cancel();

    [CascadingParameter] 
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    
    [Parameter]
    public required string ParserName { get; set; }
    
    [Inject] private OrchestratorClientService OrchestratorClient { get; set; } = null!;
    
    private const string DefaultDragClass = "relative rounded-lg border-2 border-dashed pa-4 mt-4 mud-width-full mud-height-full";
    private string _dragClass = DefaultDragClass;
    private readonly List<string> _fileNames = new();
    IList<IBrowserFile> _files = new List<IBrowserFile>();
    private MudFileUpload<IReadOnlyList<IBrowserFile>>? _fileUpload;
    
    private async Task ClearAsync()
    {
        await (_fileUpload?.ClearAsync() ?? Task.CompletedTask);
        _fileNames.Clear();
        _files.Clear();
        StateHasChanged();
        ClearDragClass();
    }
    
    //TODO: Before using, fix bug where clicking on removing file opens file picker
    private void RemoveFile(string fileName)
    {
        _fileNames.Remove(fileName);
        StateHasChanged();
        ClearDragClass();
    }
    
    private Task OpenFilePickerAsync()
        => _fileUpload?.OpenFilePickerAsync() ?? Task.CompletedTask;
    
    private void OnInputFileChanged(InputFileChangeEventArgs e)
    {
        ClearDragClass();
        var files = e.GetMultipleFiles();
        foreach (var file in files)
        {
            _fileNames.Add(file.Name);
            FilePopUp(files);
        }
    }
    
    private void UploadFiles(IReadOnlyList<IBrowserFile> files)
    {
        foreach (var file in files)
        {
            _files.Add(file);
        }
        //TODO upload the files to the server
    }
    
    private List<UploadResult> uploadResults = new();
    private List<UploadResult> files = new();
    
    private async Task Upload()
    {
        long maxFileSize = 1024 * 15;
        var upload = false;
        
        using var content = new MultipartFormDataContent();

        foreach (var file in _files)
        {
            if (uploadResults.SingleOrDefault(
                    f => f.FileName == file.Name) is null)
            {
                try
                {
                    files.Add(new UploadResult { FileName = file.Name });

                    var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));

                    fileContent.Headers.ContentType =
                        new MediaTypeHeaderValue(file.ContentType);

                    content.Add(
                        content: fileContent,
                        name: "\"files\"",
                        fileName: file.Name);
                    upload = true;
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{file.Name} not uploaded (Err: 6): {ex.Message}");

                    uploadResults.Add(
                        new()
                        {
                            FileName = file.Name,
                            ErrorCode = 6,
                            Uploaded = false
                        });
                }
            }
        }
        if (upload)
        {
            var response = await OrchestratorClient.UploadFiles(ParserName, content);
            // request.SetBrowserRequestStreamingEnabled(true);
            // request.Content = content;
            //
            // var response = await WebRequestMethods.Http.SendAsync(request);

            // var newUploadResults = await response.ReadFromJsonAsync<IList<UploadResult>>();
            Console.WriteLine(response);
            // if (newUploadResults is not null)
            // {
            //     uploadResults = uploadResults.Concat(newUploadResults).ToList();
            // }
        DialogSubmit();
        var authState = await AuthenticationStateTask;
        var authUser = authState.User;
        var user = RoleUtil.GetUserEmail(authUser);
        foreach (var file in _fileNames)
        {
            _logger.LogInformation( "[AUDIT] {User} uploaded dataset: {file} to {Parser}", user, file, ParserName);
        }
        Snackbar.Add("Uploaded your files!");
        }
        
    }
    
    private void SetDragClass()
    {
        _dragClass = $"{DefaultDragClass} mud-border-warning";
    }
    private void FilePopUp( IReadOnlyList<IBrowserFile> files)
    {
        if (files.Count == 1)
        {
            Snackbar.Add($"Added file: {files[0].Name}", Severity.Info);
            return;
        } 
        Snackbar.Add($"Added {files.Count} files", Severity.Info);
    }

    private void ClearDragClass()
    {
        _dragClass = DefaultDragClass;
    }
}