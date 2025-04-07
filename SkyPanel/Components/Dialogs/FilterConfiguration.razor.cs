using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Dialogs;

public partial class FilterConfiguration : ComponentBase
{
    [Parameter, Required]
    public string ParserName { get; set; } = string.Empty;
    private List<string> availableFilters = new();
    private List<string> selectedFilters = new();
    private string selectedFilter;
    [Inject] private OrchestratorClientService _orchestratorClientService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var filters = await _orchestratorClientService.GetFilters();
        availableFilters = filters.ToList();
    }

    private void AddFilter()
    {
        if (!string.IsNullOrEmpty(selectedFilter) && !selectedFilters.Contains(selectedFilter))
        {
            selectedFilters.Add(selectedFilter);
            selectedFilter = null;
        }
    }

    private void RemoveFilter(string filter)
    {
        selectedFilters.Remove(filter);
    }
}