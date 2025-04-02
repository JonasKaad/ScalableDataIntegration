using CommonDis.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using SkyPanel.Components.Dialogs;
using SkyPanel.Components.Models;
using SkyPanel.Components.Services;
using SkyPanel.Utils;

namespace SkyPanel.Components.Features;
 
public partial class LatestDatasetPanel : ComponentBase
{
    private readonly ILogger<LatestDatasetPanel> _logger;
    
    public LatestDatasetPanel (ILogger<LatestDatasetPanel> logger)
    {
        _logger = logger;
    }

    private List<BlobDataItem>? _blobDataItems;
    private bool _isLoading;
    
    [Inject] private StatisticsDatabaseService Db { get; set; } = default!;
    [Inject] private ParserStateService ParserState { get; set; } = default!;
    [Inject] private BlobManagerService BlobService { get; set; } = default!;
    
    [CascadingParameter] 
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;
    
    private async Task<bool> CanInteractWithParser(string? parserName)
    {
        if (string.IsNullOrEmpty(parserName))
            return false;
            
        var authState = await AuthenticationStateTask;
        var user = authState.User;
        
        // Admin can interact with all parsers
        if (user.IsInRole("Admin"))
            return true;
        
        // User can interact with parsers matching their role
        return RoleUtil.HasRole(parserName, user);
    }

   

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

        if (!await CanInteractWithParser(containerName))
        {
            Snackbar.Add("You don't have permission to download this file.", Severity.Warning);
            return;
        }
        
        try
        {
            var stream = await BlobService.DownloadBlob(containerName, blobName);
            using var st = new DotNetStreamReference(stream: stream);
            await JS.InvokeVoidAsync("downloadFileFromStream", $"{containerName}_{blobName}", st);
            var authState = await AuthenticationStateTask;
            var authUser = authState.User;
            var user = RoleUtil.GetUserEmail(authUser);
            _logger.LogInformation( "[AUDIT] {User} downloaded dataset: {Dataset} from {Parser}", user, blobName, containerName);
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
        if (blobDataItem.ParserName == null || !await CanInteractWithParser(blobDataItem.ParserName))
        {
            Snackbar.Add("You don't have permission to delete this dataset.", Severity.Warning);
            return;
        }
        
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
                var authState = await AuthenticationStateTask;
                var authUser = authState.User;
                var user = RoleUtil.GetUserEmail(authUser);
                if (!string.IsNullOrEmpty(blobDataItem.ParsedPath))
                {
                    await BlobService.DeleteBlob(blobDataItem.ParserName, blobDataItem.ParsedPath);
                    deletedDatasets.Add(blobDataItem.ParsedPath);
                    _logger.LogInformation( "[AUDIT] {User} deleted parsed dataset: {DatasetUrl} for {Parser}", user, blobDataItem.ParsedPath, blobDataItem.ParserName);
                }

                if (!string.IsNullOrEmpty(blobDataItem.RawPath))
                {
                    await BlobService.DeleteBlob(blobDataItem.ParserName, blobDataItem.RawPath);
                    _logger.LogInformation( "[AUDIT] {User} deleted raw dataset: {DatasetUrl} for {Parser}", user, blobDataItem.RawPath, blobDataItem.ParserName);
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