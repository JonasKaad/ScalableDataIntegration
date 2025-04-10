using System.ComponentModel.DataAnnotations;
using CommonDis.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
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
            StateHasChanged(); // Trigger re-render
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
        // Save the selected filters to the parser state
        MudDialog?.Close(DialogResult.Ok(true));
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
    }
}