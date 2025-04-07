using System.ComponentModel.DataAnnotations;
using CommonDis.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Dialogs;

public partial class FilterConfiguration : ComponentBase
{
    [Parameter, Required]
    public string ParserName { get; set; } = string.Empty;
    
    private List<Filter> availableFilters = new();
    private List<DropItem> dropItems = new();
    private List<Filter> selectedFilters = new();
    private List<Filter> expandedFilters = new();
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
        availableFilters = filters.ToList();
        selectedFilters = ParserState.Filters.Where(t => !string.IsNullOrWhiteSpace(t)).Select(f => new Filter { Name = f }).ToList();
        dropItems = selectedFilters.Select(f => new DropItem { Name = f.Name }).ToList();
        isLoading = false;
    }
    
    private void OpenFilterSettings(string filterName)
    {
        if (expandedFilters.Any(f => f.Name == filterName))
        {
            expandedFilters.RemoveAll(f => f.Name == filterName);
        }
        else
        {
            var param = _orchestratorClientService.GetFilterParameters(filterName).Result;
            expandedFilters.Add(new Filter()
            {
                Name = filterName,
                Parameters = param
            });
        }
    }

    private IEnumerable<string> filteredAvailableFilters => string.IsNullOrWhiteSpace(searchString)
        ? availableFilters.Except(selectedFilters).Select(f => f.Name)
        : availableFilters.Except(selectedFilters)
            .Where(f => f.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).Select(f => f.Name);



// Update AddFilter method
    private void AddFilter(string filter)
    {
        if (selectedFilters.All(f => f.Name != filter))
        {
            selectedFilters.Add(new (){Name = filter});
            dropItems.Add(new DropItem { Name = filter });
            _dropContainer.Refresh();
        }
    }

// Update RemoveFilter method
    private void RemoveFilter(string filter)
    {
        selectedFilters.RemoveAll(f => f.Name == filter);
        dropItems.RemoveAll(item => item.Name == filter);
        _dropContainer.Refresh();
    }
    
    private void SaveFilters()
    {
        // Save the selected filters to the parser state
        ParserState.Filters = selectedFilters.Select(f => f.Name).ToList();
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
    
        // Update the selectedFilters list to match
        selectedFilters = dropItems.Select(item => new Filter(){Name = item.Name}).ToList();
        _dropContainer.Refresh();
    }
    
    public class DropItem
    {
        public string Name { get; set; } = string.Empty;
        public string Selector { get; set; } = "0";
    }
}