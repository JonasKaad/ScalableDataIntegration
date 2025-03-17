using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SkyPanel.Components.Models;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Features;

public partial class LatestDatasetPanel : ComponentBase
{
    private Dataset[]? _datasets;
    [Inject] private StatisticsDatabaseService Db { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        // Simulate asynchronous loading to demonstrate a loading indicator
        //wait Task.Delay(500);
        
        

        Console.WriteLine("Querying for statistics");
        
        _datasets = await Db.Datasets
            .OrderBy(b => b.Date)
            .ToArrayAsync();
    }
}