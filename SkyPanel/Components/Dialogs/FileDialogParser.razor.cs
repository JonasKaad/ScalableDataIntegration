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

    private void Upload()
    {
        if (_files.Count == 0)
        {
            Snackbar.Add("No files selected", Severity.Warning);
            return;
        }

        Console.WriteLine(_files.Count);
        DialogSubmit(_files);
    }
    
    private void UploadFiles(IReadOnlyList<IBrowserFile>? files)
    {
        // If files are null return so it can be cleared?
        if (files == null) return;
        
        // Add files to the list
        foreach (var file in files)
        {
            _files.Add(file);
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