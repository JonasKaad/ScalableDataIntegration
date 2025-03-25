using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Dialogs;

public partial class UpdateDialog : ComponentBase
{
    [Parameter, Required]
    public required string Parser { get; set; }
    [Parameter]
    public  string Url { get; set; }= string.Empty;
    [Parameter]
    public string BackupUrl { get; set; } = string.Empty;
    [Parameter]
    public string SecretName { get; set; } = string.Empty;
    [Parameter]
    public string PollingRate { get; set; } = string.Empty;
    
    [Inject] private ParserStateService ParserState { get; set; } = null!;
    
    [CascadingParameter] IMudDialogInstance? MudDialog { get; set; }
    
    private void DialogSubmit() => MudDialog?.Close(DialogResult.Ok("update"));

    private void DialogCancel() => MudDialog?.Cancel();
    
    private string HighlightAddedContent(string? oldValue, string? newValue)
    {
        if (string.IsNullOrEmpty(newValue))
        {
            return string.Empty;
        }
    
        // If old value is empty, highlight the entire new value as added
        if (string.IsNullOrEmpty(oldValue))
        {
            return $"<span style=\"background-color: #d0f0c0; font-weight: bold;\">{newValue}</span>";
        }

        if (oldValue == newValue)
        {
            return newValue;
        }
        
        var (prefix, changed, suffix) = FindDiff(newValue, oldValue);


        return $"{prefix}<span style=\"background-color: #d0f0c0; font-weight: bold;\">{changed}</span>{suffix}";
    }

    private string HighlightRemovedContent(string? oldValue, string? newValue)
    {
        if (string.IsNullOrEmpty(oldValue) || string.IsNullOrEmpty(newValue))
        {
            return oldValue ?? string.Empty;
        }

        if (oldValue == newValue)
        {
            return oldValue;
        }

        var (prefix, changed, suffix) = FindDiff(oldValue, newValue);

        return $"{prefix}<span style=\"background-color: #ffcccc; font-weight: bold;\">{changed}</span>{suffix}";
    }
    
    // Inspired by @https://github.com/google/diff-match-patch/blob/master/csharp/DiffMatchPatch.cs
    private static (string prefix, string changed, string suffix) FindDiff(string oldValue, string newValue)
    {
        // Simple diff logic to find common prefixes and suffixes
        int prefixLength = 0;
        int minLength = Math.Min(oldValue.Length, newValue.Length);

        // Find common prefix
        while (prefixLength < minLength && oldValue[prefixLength] == newValue[prefixLength])
        {
            prefixLength++;
        }

        // Find common suffix
        int oldIndex = oldValue.Length - 1;
        int newIndex = newValue.Length - 1;
        int suffixLength = 0;

        while (suffixLength < minLength - prefixLength && 
               oldIndex >= prefixLength && 
               newIndex >= prefixLength && 
               oldValue[oldIndex] == newValue[newIndex])
        {
            oldIndex--;
            newIndex--;
            suffixLength++;
        }

        string prefix = oldValue.Substring(0, prefixLength);
        string changed = oldValue.Substring(prefixLength, oldValue.Length - prefixLength - suffixLength);
        string suffix = oldValue.Substring(oldValue.Length - suffixLength);
        
        return (prefix, changed, suffix);
    }
}