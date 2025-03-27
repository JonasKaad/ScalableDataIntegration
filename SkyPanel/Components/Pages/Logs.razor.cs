using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Web;
using Microsoft.JSInterop;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Pages;

public partial class Logs
{
    
    [Inject] private OrchestratorClientService OrchestratorClient { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    
    private readonly List<string> _severities = new() 
    { 
        "error", 
        "warn", 
        "info", 
        "debug" 
    };
    
    private readonly List<string> _sources = new()
    {
        "csharp",
        "javascript",
        "python",
        "java",
        "ruby"
    };
    private List<string> _services = new() { "DownloadOrchestrator" };
    private DateRange _dateRange = new(DateTime.Today.AddDays(-1), DateTime.Today);
    private IEnumerable<string> _selectedServices = new List<string>();
    private IEnumerable<string> _selectedSeverities = new List<string>();
    private string? _datadogUrl;
    private bool _loading = true;
    private IEnumerable<string> _selectedSources = new List<string>();
    private TimeSpan? _fromTime = new TimeSpan(00, 00, 00);
    private TimeSpan? _toTime = new TimeSpan(23, 59, 59);
    

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var downloaders = await OrchestratorClient.GetDownloaders();
            
            foreach (var downloader in downloaders)
            {
                _services.Add(downloader);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching downloaders: {ex.Message}");
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
        _selectedSeverities = _severities;
    }
    
    private async Task<IEnumerable<string>>? SearchServices(string? searchText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(searchText))
            return _services;
    
        return _services
            .Where(s => s.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task OpenDatadog()
    {
        string url = GenerateDatadogUrl();
        await JSRuntime.InvokeVoidAsync("open", url, "_blank");
    }

    private string GenerateDatadogUrl()
    {
        string baseUrl = "https://us5.datadoghq.com/logs";
        
        var queryParams = new Dictionary<string, string>();
        var queryParts = new List<string>();
        
        if (_selectedServices.Any())
        {
            queryParts.Add($"service:({string.Join(" OR ", _selectedServices)})");
        }
        
        if (_selectedSources.Any())
        {
            queryParts.Add($"source:({string.Join(" OR ", _selectedSources)})");
        }
        
        if (_selectedSeverities.Any())
        {
            queryParts.Add($"status:({string.Join(" OR ", _selectedSeverities)})");
        }
        
        if (queryParts.Any())
        {
            queryParams.Add("query", string.Join(" ", queryParts));
        }
        
        queryParams.Add("agg_m", "count");
        queryParams.Add("agg_m_source", "base");

        if (_dateRange is { Start: not null, End: not null })
        {
            var startDateTime = _dateRange.Start.Value.Add(_fromTime.GetValueOrDefault());
            var endDateTime = _dateRange.End.Value.Add(_toTime.GetValueOrDefault());
    
            var startTs = new DateTimeOffset(startDateTime).ToUnixTimeMilliseconds();
            var endTs = new DateTimeOffset(endDateTime).ToUnixTimeMilliseconds();

            queryParams.Add("from_ts", startTs.ToString());
            queryParams.Add("to_ts", endTs.ToString());
        }

        // Construct URL
        string queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));
        return $"{baseUrl}?{queryString}";
    }
}