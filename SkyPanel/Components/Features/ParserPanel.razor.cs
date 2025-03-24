using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Dialogs;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Features;

public partial class ParserPanel : ComponentBase
{
    [Inject] private ParserStateService ParserState { get; set; } = null!;
    [Inject] private BlobManagerService BlobService { get; set; } = null!;
    
    private Parser[] _parsers =
    [
        new("1", "https://www.google.com", "Http", "24", "https://www.google.dk"),
        new("2",  "ftp://www.test.com", "Ftp", "37"),
        get => ParserState.ParserName;

    //TODO: Currently just a placeholder method, will be replaced with actual parser fetching
    private void FetchParsers()
    {
        var containerNames = BlobService.GetContainerNames().ToArray();
    
        // Resize array if needed to accommodate all containers
        if (_parsers.Length < containerNames.Length)
        {
            Array.Resize(ref _parsers, containerNames.Length);
        }

        // Update parsers with container names
        for (int i = 0; i < containerNames.Length; i++)
        {
            string containerName = containerNames[i];
        
            // If parser at this index is null, create a new one
            if (_parsers[i] == null)
            {
                _parsers[i] = new Parser(i.ToString(), "https://jsonplaceholder.typicode.com/todos/1", "Http", "13")
                {
                    Name = containerName
                };
            }
            else
            {
                // Update existing parser name
                _parsers[i].Name = containerName;
            }
        }
    }

    private Parser[] GetParsers()
    {
        FetchParsers();
        return _parsers;
    }
    
    
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
            { x => x.ParserName, ParserState.ParserName },
            
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
            { x => x.EmphasizedCenterText, ParserState.ParserName },
            { x => x.SnackbarMessage, $"Started fetching and parsing latest dataset for {ParserState.ParserName}"},
            { x => x.SnackbarSeverity, Severity.Info },
        };
        
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall, FullWidth = true };
        return DialogService.ShowAsync<BasicDialog>("Fetch and parse latest dataset", parameters, options);
    }
    
    private async Task<IEnumerable<string>> Search(string value, CancellationToken token)
    {
        var downloaders = await OrchestratorClient.GetDownloaders();
        
        if (string.IsNullOrEmpty(value))
        { 
           return downloaders;
        }

        return downloaders.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }
}