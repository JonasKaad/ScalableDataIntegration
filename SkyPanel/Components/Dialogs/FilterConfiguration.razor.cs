using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Dialogs;

public partial class FilterConfiguration : ComponentBase
{
    [Parameter, Required]
    public string ParserName { get; set; } = string.Empty;
    
    private List<string> availableFilters = new();
    private List<DropItem> dropItems = new();
    private List<string> selectedFilters = new();
    private bool isLoading = true;
    private string searchString = string.Empty;

    private MudDropContainer<DropItem> _dropContainer;

    [Inject] private ParserStateService ParserState { get; set; } = null!;
    [Inject] private OrchestratorClientService _orchestratorClientService { get; set; } = null!;
    [CascadingParameter] IMudDialogInstance? MudDialog { get; set; }

    private IEnumerable<string> filteredAvailableFilters => string.IsNullOrWhiteSpace(searchString)
        ? availableFilters.Except(selectedFilters)
        : availableFilters.Except(selectedFilters)
            .Where(f => f.Contains(searchString, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        var filters = await _orchestratorClientService.GetFilters();
        availableFilters = filters.ToList();
        selectedFilters = ParserState.Filters;
        dropItems = selectedFilters.Select(f => new DropItem { Name = f }).ToList();
        isLoading = false;
    }

// Update AddFilter method
    private void AddFilter(string filter)
    {
        if (!selectedFilters.Contains(filter))
        {
            selectedFilters.Add(filter);
            dropItems.Add(new DropItem { Name = filter });
            _dropContainer.Refresh();
        }
    }

// Update RemoveFilter method
    private void RemoveFilter(string filter)
    {
        selectedFilters.Remove(filter);
        dropItems.RemoveAll(item => item.Name == filter);
        _dropContainer.Refresh();
    }
    
    private void SaveFilters()
    {
        // Save the selected filters to the parser state
        ParserState.Filters = selectedFilters;
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
        selectedFilters = dropItems.Select(item => item.Name).ToList();
        _dropContainer.Refresh();
    }
    
    public class DropItem
    {
        public string Name { get; set; } = string.Empty;
        public string Selector { get; set; } = "0";
    }
}