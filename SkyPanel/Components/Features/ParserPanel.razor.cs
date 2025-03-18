using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Dialogs;
using SkyPanel.Components.Models;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Features;

public partial class ParserPanel : ComponentBase
{
    [Inject] private ParserStateService ParserState { get; set; } = null!;
    [Inject] private BlobManagerService BlobService { get; set; } = null!;
    
    private Parser[] _parsers =
    [
        new("1", "https://www.google.com", "Http", 24),
        new("2",  "ftp://www.test.com", "Ftp", 37),
        new("3",  "https://jsonplaceholder.typicode.com/todos/1", "Http", 12)
    ];

    private void FetchParsers()
    {
        foreach (var (containerName, index) in BlobService.GetContainerNames().Select((x, i) => (x, i)))
        {
            _parsers[index].Name = containerName;
        }
    }

    private Parser[] GetParsers()
    {
        FetchParsers();
        return _parsers;
    }
    
    private string ParserName => ParserState.ParserName;
    
    public string Parser
    {
        get => ParserState.ParserName;
        set => ParserState.SetParser(GetParsers().FirstOrDefault(x => x.Name == value));
    }
    
    protected override void OnInitialized()
    {
        ParserState.OnChange += StateHasChanged;
    }
    
    private Task OpenFileDialogAsync()
    {
        var parameters = new DialogParameters<FileDialogParser>
        {
            { x => x.ParserName, ParserName },
            
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        return DialogService.ShowAsync<FileDialogParser>("Upload Dataset", parameters, options);
    }

    private Task OpenReparseDialogAsync()
    {
        var parameters = new DialogParameters<BasicDialog>
        {
            { x => x.ContentText, "Do you want to fetch and parse the latest dataset for:" },
            { x => x.ConfirmationButtonText, "Confirm" },
            { x => x.EmphasizedCenterText, ParserName },
            { x => x.SnackbarMessage, $"Started fetching and parsing latest dataset for {ParserName}"},
            { x => x.SnackbarSeverity, Severity.Info },
        };
        
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall, FullWidth = true };
        return DialogService.ShowAsync<BasicDialog>("Fetch and parse latest dataset", parameters, options);
    }
    
    private Task<IEnumerable<string>> Search(string value, CancellationToken token)
    {
        
        // if text is null or empty, show complete list
        if (string.IsNullOrEmpty(value))
        { 
            var temp = Task.FromResult<IEnumerable<Parser>>(GetParsers()); 
            return Task.FromResult(temp.Result.Select(x => x.Name));
        }
        else
        {
            var temp = Task.FromResult<IEnumerable<Parser>>(GetParsers());
            return Task.FromResult(temp.Result.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Name));
        }
    }
}