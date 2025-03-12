using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SkyPanel.Components.Modals;

public partial class TextModal : ComponentBase
{
    [Parameter]
    public string ContentText { get; set; } = string.Empty;
    
    [Parameter]
    public string ConfirmationButtonText { get; set; } = "Ok";
    
    [Parameter]
    public Color Color { get; set; } = Color.Success;
    
    
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    private void DialogSubmit() => MudDialog.Close(DialogResult.Ok(true));
    private void DialogCancel() => MudDialog.Cancel();
}