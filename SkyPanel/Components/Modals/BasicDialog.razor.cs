using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SkyPanel.Components.Modals;

public partial class BasicDialog : ComponentBase
{
    [Parameter, EditorRequired]
    public required string ContentText { get; set; }
    
    [Parameter]
    public string EmphasizedCenterText { get; set; } = string.Empty;
    
    [Parameter]
    public string SnackbarMessage { get; set; } = string.Empty;

    [Parameter] public Severity SnackbarSeverity { get; set; } = Severity.Normal;
    
    [Parameter]
    public string ConfirmationButtonText { get; set; } = "Ok";
    
    [Parameter]
    public Color Color { get; set; } = Color.Success;
    
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }
    private void DialogSubmit()
    {
        MudDialog?.Close(DialogResult.Ok(true));
        if (!string.IsNullOrEmpty(SnackbarMessage))
        {
            Snackbar.Add(SnackbarMessage, SnackbarSeverity);
        }

    }

    private void DialogCancel() => MudDialog?.Cancel();
}