using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Dialogs;

public partial class UpdateDialog : ComponentBase
{
    [Parameter, Required]
    public required string Parser { get; set; }
    [Parameter]
    public  string Url { get; set; }= string.Empty;
    [Parameter]
    public string BackupUrl { get; set; } = string.Empty;
    [Parameter]
    public string SecretName { get; set; } = string.Empty;
    [Parameter]
    public string PollingRate { get; set; } = string.Empty;
    
    [Inject] private ParserStateService ParserState { get; set; } = null!;
    
    [CascadingParameter] IMudDialogInstance? MudDialog { get; set; }
    
    private void DialogSubmit() => MudDialog?.Close(DialogResult.Ok("update"));

    private void DialogCancel() => MudDialog?.Cancel();
    
}