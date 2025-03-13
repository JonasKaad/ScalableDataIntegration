using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Dialogs;
using SkyPanel.Components.Models;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Features;

public partial class ParserPanel : ComponentBase, IDisposable
{
    [Inject] private ParserStateService ParserState { get; set; } = default!;

    private string ParserName
    {
        get => ParserState.ParserName;
    }

    class ParserWrapper
    {
       
    }
    public string Parser
    {
        get => ParserState.ParserName;
        set => ParserState.SetParser(ParserState.TestParsers.FirstOrDefault(x => x.Name == value));
    }
    
    protected override void OnInitialized()
    {
        ParserState.OnChange += StateHasChanged;
        //_parserName = ParserState.ParserName;
    }
    
    public void Dispose()
    {
        ParserState.OnChange -= StateHasChanged;
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
}