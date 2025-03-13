using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Dialogs;

namespace SkyPanel.Components.Features;

public partial class ParserPanel : ComponentBase
{
    public required string ParserName { get; set; }
    
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