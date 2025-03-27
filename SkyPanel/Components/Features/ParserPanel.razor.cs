using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Dialogs;
using SkyPanel.Components.Services;

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