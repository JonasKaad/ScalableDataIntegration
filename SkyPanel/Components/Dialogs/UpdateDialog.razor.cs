using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Dialogs;

public partial class BlobDeleteDialog : ComponentBase
{
    [Parameter]
    public  string RawDataset { get; set; }= string.Empty;
    
    [Parameter]
    public string ParsedDataset { get; set; } = string.Empty;
    
    [Parameter, Required]
    public required string Parser { get; set; }
    
    [Inject] private BlobManagerService BlobService { get; set; } = default!;

    [CascadingParameter] IMudDialogInstance? MudDialog { get; set; }
    
    private void DialogSubmit() => MudDialog?.Close(DialogResult.Ok("delete"));

    private void DialogCancel() => MudDialog?.Cancel();
    
}