using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SkyPanel.Utils;

namespace SkyPanel.Components.Dialogs;

public partial class FileDialogParser : ComponentBase
{
    private readonly ILogger<FileDialogParser> _logger;
    
    public FileDialogParser (ILogger<FileDialogParser> logger)
    {
        _logger = logger;
    }

    [CascadingParameter] 
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    
    [Parameter]
    public required string ParserName { get; set; }
    
    private const string DefaultDragClass = "relative rounded-lg border-2 border-dashed pa-4 mt-4 mud-width-full mud-height-full";
    private string _dragClass = DefaultDragClass;
    private readonly List<string> _fileNames = new();
    private MudFileUpload<IReadOnlyList<IBrowserFile>>? _fileUpload;
    
    private async Task ClearAsync()
    {
        await (_fileUpload?.ClearAsync() ?? Task.CompletedTask);
        _fileNames.Clear();
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
    
    private async Task Upload()
    {
        // TODO: Implement logic for handling file uploads
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