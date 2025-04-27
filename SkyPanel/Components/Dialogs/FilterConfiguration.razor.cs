using System.ComponentModel.DataAnnotations;
using CommonDis.Models;
using Cropper.Blazor.Components;
using Cropper.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Charts;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Dialogs;

public partial class FilterConfiguration : ComponentBase
{
    private List<FilterDto> availableFilters = new();
    private List<DropItem> dropItems = new();
    [Parameter, Required]
    public List<FilterDto> SelectedFilters { get; set; } = new();
    private List<FilterDto> expandedFilters = new();
    private bool isLoading = true;
    private string searchString = string.Empty;
    private string? uploadedImageSrc;
    private bool hasUploadedImage = false;
    private CropperComponent? _cropper = null;
    private bool _cropSaved = false;
    private Options CropperOptions = new Options()
    {
        SetDataOptions = new SetDataOptions()
        {
            Width = 400,
            Height = 400
        },
        ViewMode = ViewMode.Vm1
    };
    
    decimal _cropBoxWidth = 400;
    decimal _cropBoxHeight = 400;
    decimal _cropBoxLeft = 400;
    decimal _cropBoxTop = 400;

    private MudDropContainer<DropItem> _dropContainer;

    [Inject] private ParserStateService ParserState { get; set; } = null!;
    [Inject] private OrchestratorClientService _orchestratorClientService { get; set; } = null!;
    [CascadingParameter] IMudDialogInstance? MudDialog { get; set; }

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        var filters = await _orchestratorClientService.GetFilters();
        availableFilters = new List<FilterDto>();

        foreach (var filter in filters)
        {
            var parameters = await _orchestratorClientService.GetFilterParameters(filter);
            availableFilters.Add(new FilterDto()
            {
                Name = filter,
                Parameters = parameters
            });
        }

        dropItems = SelectedFilters.Select(f => new DropItem { Filter = f }).ToList();
        isLoading = false;
        StateHasChanged();
    }
    
    private void OpenFilterSettings(FilterDto filter)
    {
        if (!expandedFilters.Remove(filter))
        {
            expandedFilters.Add(filter);
        }

        var filterWithParameters = expandedFilters.FirstOrDefault(f => 
            f.Parameters.ContainsKey("startX") && f.Parameters.ContainsKey("startY")
            && f.Parameters.ContainsKey("endX") && f.Parameters.ContainsKey("endY"));

        var canUpdateCrop = filterWithParameters != null;
        if (canUpdateCrop && _cropper is not null)
        {
            SetCropBox(filterWithParameters.Parameters);
        }
        StateHasChanged();
    }
    
    private async Task SaveCrop()
    {
        var cropData = await _cropper.GetDataAsync(false, CancellationToken.None);
        _cropBoxHeight = cropData.Height ?? new decimal(0);
        _cropBoxWidth = cropData.Width ?? new decimal(0);
        _cropBoxLeft = cropData.X ?? new decimal(0);
        _cropBoxTop = cropData.Y ?? new decimal(0);
        _cropSaved = true;
        StateHasChanged();
    
        await Task.Delay(2000); // Show for 2 seconds
        _cropSaved = false;
        StateHasChanged();
    }

    private IEnumerable<FilterDto> filteredAvailableFilters => string.IsNullOrWhiteSpace(searchString)
        ? availableFilters.Where(f => SelectedFilters.All(sf => sf.Name != f.Name))
        : availableFilters.Where(f => SelectedFilters.All(sf => sf.Name != f.Name) &&
                                      f.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));

    private void AddFilter(FilterDto filter)
    {
        if (SelectedFilters.All(f => f.Name != filter.Name))
        {
            SelectedFilters.Add(filter);
            dropItems.Add(new DropItem() { Filter = filter });
            _dropContainer.Refresh();
            StateHasChanged();
            Console.WriteLine($"Added filter: {filter.Name}");
        }
    }

    private void RemoveFilter(FilterDto filter)
    {
        SelectedFilters.Remove(filter);
        dropItems.RemoveAll(item => item.Filter == filter);
        _dropContainer.Refresh();
    }
    
    private void SaveFilters()
    {
        var newFilters = new List<FilterDto>();
        newFilters = SelectedFilters;
        if (_cropper != null)
        {
            var coordinates = new Dictionary<string, decimal?>
            {
                { "startX", _cropBoxLeft },
                { "startY", _cropBoxTop},
                { "endX", _cropBoxWidth + _cropBoxLeft },
                { "endY", _cropBoxTop + _cropBoxHeight }
            };

            foreach (var filter in newFilters)
            {
                // This might have place unintended values if multiple filters use startX/Y or endX/Y
                foreach (var key in coordinates.Keys.Intersect(filter.Parameters.Keys))
                {
                    filter.Parameters[key] = coordinates[key]?.ToString();
                }
            }
        }
    
        MudDialog?.Close(DialogResult.Ok(newFilters));
    }
    
    private async Task HandleImageUpload(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            await using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var base64Image = Convert.ToBase64String(memoryStream.ToArray());
            uploadedImageSrc = $"data:{file.ContentType};base64,{base64Image}";
            hasUploadedImage = true;
            StateHasChanged();
            if (_cropper is not null)
            {
                var filterWithParameters = expandedFilters.FirstOrDefault(f => 
                    f.Parameters.ContainsKey("startX") && f.Parameters.ContainsKey("startY")
                    && f.Parameters.ContainsKey("endX") && f.Parameters.ContainsKey("endY"));
                SetCropBox(filterWithParameters.Parameters);
            }
        }
    }

    private void SetCropBox(Dictionary<string, string> filterParameters)
    {
        var startY = filterParameters["startY"];
        var startX = filterParameters["startX"];
        var endY = filterParameters["endY"];
        var endX = filterParameters["endX"];
        
        var x = decimal.TryParse(startX, out var xVal) ? xVal : _cropBoxLeft;
        var y = decimal.TryParse(startY, out var yVal) ? yVal : _cropBoxTop;
        
        var height = decimal.TryParse(startY, out var startYValue) && decimal.TryParse(endY, out var endYVal)
            ? endYVal - startYValue
            : _cropBoxHeight;
        
        var width = decimal.TryParse(startX, out var startXValue) && decimal.TryParse(endX, out var endXVal)
            ? endXVal - startXValue
            : _cropBoxWidth;
        
        _cropper.Options.SetDataOptions.Height = height;
        _cropper.Options.SetDataOptions.Width = width;
        _cropper.Options.SetDataOptions.X = x;
        _cropper.Options.SetDataOptions.Y = y;
    }
    
    private string CalculateParameterHeight(int count)
    {
        return $"height: {count * 75}px";
    }

    private void ItemUpdated(MudItemDropInfo<DropItem> dropItem)
    {
        // Rearrange the DropItems list
        int index = dropItems.IndexOf(dropItem.Item);
        dropItems.RemoveAt(index);
    
        int newIndex = dropItem.IndexInZone;
        if (newIndex >= dropItems.Count)
            dropItems.Add(dropItem.Item);
        else
            dropItems.Insert(newIndex, dropItem.Item);
    
        // Update the SelectedFilters list to match
        SelectedFilters = dropItems.Select(d => d.Filter).ToList();
        _dropContainer.Refresh();
    }
    
    public class DropItem
    {
        public FilterDto Filter { get; set; } = new();
        public string id { get; set; } = "0";
    }
}