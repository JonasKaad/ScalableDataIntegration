using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SkyPanel.Components.Modals;

namespace SkyPanel.Components.Features;

public partial class ParserPanel : ComponentBase
{
    private Task OpenDialogAsync()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        return DialogService.ShowAsync<FileDialogParser>("Upload Dataset", options);
    }
}