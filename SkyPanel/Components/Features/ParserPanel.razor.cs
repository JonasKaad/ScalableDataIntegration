using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SkyPanel.Components.Modals;

namespace SkyPanel.Components.Features;

public partial class ParserPanel : ComponentBase
{
    private Task OpenDialogAsync()
    private Task OpenFileDialogAsync()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        return DialogService.ShowAsync<FileDialogParser>("Upload Dataset", options);
    }

    private Task OpenReparseDialogAsync()
    {
        var parameters = new DialogParameters<TextModal>
        {
            { x => x.ContentText, "Do you want to fetch and reparse the latest data for: \n" + ParserName },
            { x => x.ConfirmationButtonText, "Ok" },
            { x => x.Color, Color.Success }
        };
        
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall, FullWidth = true };
        
        return DialogService.ShowAsync<TextModal>("Fetch and parse latest dataset", parameters, options);
    }
}