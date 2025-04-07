using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SkyPanel.Components.Dialogs;

public partial class ReparseDialog : ComponentBase
{
    [Parameter, EditorRequired]
    public required string ContentText { get; set; }
    
    [Parameter]
    public string EmphasizedCenterText { get; set; } = string.Empty;
    
    [Parameter]
    public string ConfirmationButtonText { get; set; } = "Ok";
    
    [Parameter]
    public Color Color { get; set; } = Color.Success;
    
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }
    private void DialogSubmit()
    {
        MudDialog?.Close(DialogResult.Ok("reparse"));

    }

    private void DialogCancel() => MudDialog?.Cancel();
}