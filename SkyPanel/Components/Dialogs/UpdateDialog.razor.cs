using System.ComponentModel.DataAnnotations;
using CommonDis.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Services;
using SkyPanel.Utils;

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
    [Parameter]
    public List<FilterDto> Filters { get; set; } = new List<FilterDto>();
    
    private SnackbarUtil SnackbarUtil { get; set; } = new();
    
    private bool _hasChanges;
    private bool HasChanges 
    {
        get => _hasChanges; 
        set 
        {
            if (_hasChanges != value)
            {
                _hasChanges = value;
                StateHasChanged();
            }
        }
    }
    [Inject] private ParserStateService ParserState { get; set; } = null!;
    [Inject] private OrchestratorClientService OrchestratorClientService { get; set; } = null!;
    
    [CascadingParameter] IMudDialogInstance? MudDialog { get; set; }

    private async Task TestConnection()
    {
        if (string.IsNullOrEmpty(ParserState.ParserName))
        {
            return;
        }

        var urlValueToSend = "";
        var backupUrlValueToSend = "";
        var secretNameToSend = "";
        var pollingRateToSend = "";
        
        // Check if parser values have changed. If not send a string without a space: ""
        if (!string.IsNullOrEmpty(Url))
        {
            urlValueToSend = Url;
        }
        
        if (!string.IsNullOrEmpty(BackupUrl))
        {
            backupUrlValueToSend = BackupUrl;
        }
        
        secretNameToSend = SecretName;
        
        pollingRateToSend = PollingRate;
        var filters = ParserState.Filters;
        
        var response = await OrchestratorClientService.TestConnection(ParserState.ParserName, 
            urlValueToSend, backupUrlValueToSend, secretNameToSend, pollingRateToSend, filters);
        
        switch (response.Count)
        {
            case 0:
                Snackbar.Add("Failed getting a response from downloader", Severity.Error);
                break;
            case 1:
                Snackbar.Add(SnackbarUtil.FormatConnectionResponse(response.FirstOrDefault(), "URL", Url).Item1, 
                    SnackbarUtil.FormatConnectionResponse(response.FirstOrDefault(), "URL", Url).Item2);
                break;
            case 2:
                Snackbar.Add(SnackbarUtil.FormatConnectionResponse(response[0], "URL", Url).Item1, 
                    SnackbarUtil.FormatConnectionResponse(response[0], "URL", Url).Item2);
                
                Snackbar.Add(SnackbarUtil.FormatConnectionResponse(response[1], "BackupURL", BackupUrl).Item1, 
                    SnackbarUtil.FormatConnectionResponse(response[1], "BackupURL", BackupUrl).Item2);
                break;
            default:
                Snackbar.Add("Failed getting a correct response from downloader", Severity.Error);
                break;
        }
    }

    private bool AreFiltersEqual(List<FilterDto> list1, List<FilterDto> list2)
    {
        if (list1.Count != list2.Count)
            return false;
        
        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i].Name != list2[i].Name)
                return false;
            
            // Compare parameters if needed
            var params1 = list1[i].Parameters;
            var params2 = list2[i].Parameters;
        
            if (params1.Count != params2.Count)
                return false;
            
            foreach (var key in params1.Keys)
            {
                if (!params2.ContainsKey(key) || params1[key] != params2[key])
                    return false;
            }
        }
    
        return true;
    }

    
    private void DialogSubmit() => MudDialog?.Close(DialogResult.Ok("update"));

    private void DialogCancel() => MudDialog?.Cancel();
    
    private string HighlightChangedContent(object? oldValue, object? newValue, string color)
    {
        var correctOldVal = GetCorrectedVal(oldValue);
        var correctNewVal = GetCorrectedVal(newValue);
        if (correctOldVal == correctNewVal)
        {
            return correctOldVal;
        }
        var (prefix, changed, suffix) = FindDiff(correctOldVal, correctNewVal);
        return $"{prefix}<span style=\"background-color: {color}; font-weight: bold;\">{changed}</span>{suffix}";
    }

    private static string GetCorrectedVal(object? val)
    {
        var correctVal = "";
        if (val is List<FilterDto> filters)
        {
            correctVal = string.Join(", ",filters.Select(f => f.Name));
        }
        else if (val is string old)
        {
            correctVal = old;
        }
        else
        {
            throw new InvalidCastException($"Can't cast {val?.GetType()} to string");
        }

        return correctVal;
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