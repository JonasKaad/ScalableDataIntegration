using CommonDis.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SkyPanel.Components.Dialogs;

public partial class CredentialsDialog : ComponentBase
{
    [Parameter]
    public  string TokenName { get; set; }= string.Empty;
    
    [Parameter]
    public string Token { get; set; } = string.Empty;
    
    [Parameter]
    public string SecretName { get; set; } = string.Empty;
    
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    private void DialogSubmit()
    {
        DisSecret newSecret = new DisSecret
        {
            TokenName = TokenName,
            Token = Token
        };
        MudDialog?.Close(DialogResult.Ok((newSecret)));
    }

    private void DialogCancel() => MudDialog?.Cancel();
}