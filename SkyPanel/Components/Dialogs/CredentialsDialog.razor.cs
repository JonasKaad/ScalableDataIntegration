using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SkyPanel.Components.Dialogs;

public partial class CredentialsDialog : ComponentBase
{
    [Parameter]
    public  string Username { get; set; }= string.Empty;
    
    [Parameter]
    public string Password { get; set; } = string.Empty;
    
    
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }
    private void DialogSubmit()
    {
        MudDialog?.Close(DialogResult.Ok(true));
    }

    private void DialogCancel() => MudDialog?.Cancel();
}