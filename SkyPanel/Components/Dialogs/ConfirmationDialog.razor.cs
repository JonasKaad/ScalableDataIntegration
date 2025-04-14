using System.Net;
using System.Net.Http.Headers;
using CommonDis.Models.Auth0;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SkyPanel.Utils;
using SkyPanel.Components.Models;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Dialogs;

public partial class ConfirmationDialog : ComponentBase
{
    [Parameter]
    public string User { get; set; }= string.Empty;
    
    [Parameter]
    public string[] RolesToRemove { get; set; } = [];
    [Parameter]
    public string[] RolesToAdd { get; set; } = [];
    
    
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }
    private void DialogSubmit()
    {
        MudDialog?.Close(DialogResult.Ok("update"));
    }

    private void DialogCancel() => MudDialog?.Cancel();
}