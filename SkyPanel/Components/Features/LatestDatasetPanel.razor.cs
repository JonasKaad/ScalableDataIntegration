using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotNetEnv;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using SkyPanel.Components.Dialogs;
using SkyPanel.Components.Models;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Features;
 
public partial class LatestDatasetPanel : ComponentBase
{

    private List<BlobDataItem>? _blobDataItems;
    private bool _isLoading;
    
    [Inject] private StatisticsDatabaseService Db { get; set; } = default!;
    [Inject] private ParserStateService ParserState { get; set; } = default!;
    [Inject] private BlobManagerService BlobService { get; set; } = default!;
    
    [Inject]
    private IDialogService DialogService { get; set; } = default!;
    
    private void OnParserStateChanged()
    {
        _searchString = ParserState.ParserName;
        StateHasChanged();
    }
    
    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        ParserState.OnChange += OnParserStateChanged;
        _searchString = ParserState.ParserName;
        _blobDataItems = await BlobService.SetupAndReturnBlobs();
        _blobDataItems = _blobDataItems.OrderByDescending(t => t.Date).ToList();
        _isLoading = false;
    }
    
    private async Task RefreshData()
    {
        _isLoading = true;
        _blobDataItems = await BlobService.RefreshBlobsAsync();
        _blobDataItems = _blobDataItems.OrderByDescending(t => t.Date).ToList();
        _isLoading = false;
        StateHasChanged();
    }

    private async Task Download(string? containerName, string? blobName)
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
    
    private async Task OpenBlobDialogAsync(BlobDataItem blobDataItem)
    {
           
        var parameters = new DialogParameters<BlobDeleteDialog>
        {
            { x => x.RawDataset, blobDataItem.RawPath},
            { x => x.ParsedDataset, blobDataItem.ParsedPath},
            { x => x.Parser, blobDataItem.ParserName}
            
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialogResult = await (await DialogService.ShowAsync<BlobDeleteDialog>("Delete confirmation", parameters, options)).Result;
        var result = dialogResult?.Data as string ?? string.Empty;
        
        if (result == "delete")
        {
            var deletedDatasets = new List<string>();
            if (blobDataItem.ParserName != null)
            {
                if (!string.IsNullOrEmpty(blobDataItem.ParsedPath))
                {
                    await BlobService.DeleteBlob(blobDataItem.ParserName, blobDataItem.ParsedPath);
                    deletedDatasets.Add(blobDataItem.ParsedPath);
                }

                if (!string.IsNullOrEmpty(blobDataItem.RawPath))
                {
                    await BlobService.DeleteBlob(blobDataItem.ParserName, blobDataItem.RawPath);
                    deletedDatasets.Add(blobDataItem.RawPath);
                }
            }
            switch (deletedDatasets.Count)
            {
                case 1:
                    Snackbar.Add($"Deleted dataset: {deletedDatasets[0]}", Severity.Info);
                    break;
                case > 1:
                    Snackbar.Add($"Deleted datasets: {string.Join(", ", deletedDatasets)}", Severity.Info);
                    break;
            }

            await RefreshData();
        }
    }
}