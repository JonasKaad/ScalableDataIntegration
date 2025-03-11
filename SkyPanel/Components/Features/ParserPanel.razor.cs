using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace SkyPanel.Components.Features;

public partial class ParserPanel : ComponentBase
{
    private Task OpenDialogAsync()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };

        return DialogService.ShowAsync<FileDialogParser>("Simple Dialog", options);
    }
}