using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotNetEnv;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
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
        BlobService.GetContainers();
        await BlobService.GetAllBlobItems();
        
        BlobService.CreateBlobDataItems();
        BlobService.PrintDictionary();
        BlobService.PrintBlobDataItems();
        _blobDataItems = BlobService.GetBlobDataItems();
    }
}