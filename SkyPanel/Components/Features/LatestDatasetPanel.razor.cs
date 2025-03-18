using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotNetEnv;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
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

    private async Task Download(string containerName, string blobName)
    {
        var stream = await BlobService.DownloadBlob(containerName, blobName);
        using var st = new DotNetStreamReference(stream: stream, true);
        await JS.InvokeVoidAsync("downloadFileFromStream", $"{containerName}_{blobName}", st);
    }
}