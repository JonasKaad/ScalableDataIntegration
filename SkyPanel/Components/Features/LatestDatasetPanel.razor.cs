using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotNetEnv;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using SkyPanel.Components.Models;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Features;

public partial class LatestDatasetPanel : ComponentBase
{

    private List<BlobDataItem>? _blobDataItems;
    
    
    [Inject] private StatisticsDatabaseService Db { get; set; } = default!;

    [Inject] private BlobManagerService BlobService { get; set; } = default!;
    
    protected override async Task OnInitializedAsync()
    {
        _blobDataItems = await BlobService.SetupAndReturnBlobs();
    }
    
    private async Task RefreshData()
    {
        _blobDataItems = await BlobService.RefreshBlobsAsync();
        StateHasChanged();
    }

    private async Task Download(string containerName, string? blobName)
    {
        if (string.IsNullOrEmpty(blobName))
            return;

        try
        {
            var stream = await BlobService.DownloadBlob(containerName, blobName);
            using var st = new DotNetStreamReference(stream: stream);
            await JS.InvokeVoidAsync("downloadFileFromStream", $"{containerName}_{blobName}", st);
        }
        catch (Exception e)
        {
            Snackbar.Add("Failed to download file.\nRefreshing with latest data.", Severity.Error);
            await RefreshData();
            Console.WriteLine(e);
        }
    }
}