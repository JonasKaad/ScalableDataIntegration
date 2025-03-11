using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace SkyPanel.Components.Features;

public partial class FileDialogParser : ComponentBase
{
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
            FilePopUp(file.Name);
        }
    }
    
    private void Upload()
    {
        // TODO: Implement logic for handling file uploads
        DialogSubmit();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
        Snackbar.Add("Uploaded your files!");
    }
    
    private void SetDragClass()
    {
        _dragClass = $"{DefaultDragClass} mud-border-warning";
    }
    private void FilePopUp(string file) => Snackbar.Add($"Added file: {file}", Severity.Success );

    private void ClearDragClass()
    {
        _dragClass = DefaultDragClass;
    }
}