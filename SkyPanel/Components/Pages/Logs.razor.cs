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
    private List<string> _services = new() { "DownloadOrchestrator", "SkyPanel" };
    private DateRange _dateRange = new(DateTime.Today.AddDays(-1), DateTime.Today);
    private IEnumerable<string> _selectedServices = new List<string>();
    private IEnumerable<string> _selectedSeverities = new List<string>();
    private string? _datadogUrl;
    private bool _loading = true;
    private IEnumerable<string> _selectedSources = new List<string>();
    private TimeSpan? _fromTime = new TimeSpan(00, 00, 00);
    private TimeSpan? _toTime = new TimeSpan(23, 59, 59);
    
    public DateRange DateRange
    {
        get => _dateRange;
        set
        {
            _dateRange = value;
            UpdateDatadogUrl();
        }
    }

    public IEnumerable<string> SelectedServices
    {
        get => _selectedServices;
        set
        {
            _selectedServices = value;
            UpdateDatadogUrl();
        }
    }

    public IEnumerable<string> SelectedSeverities
    {
        get => _selectedSeverities;
        set
        {
            _selectedSeverities = value;
            UpdateDatadogUrl();
        }
    }

    public IEnumerable<string> SelectedSources
    {
        get => _selectedSources;
        set
        {
            _selectedSources = value;
            UpdateDatadogUrl();
        }
    }

    public TimeSpan? FromTime
    {
        get => _fromTime;
        set
        {
            _fromTime = value;
            UpdateDatadogUrl();
        }
    }

    public TimeSpan? ToTime
    {
        get => _toTime;
        set
        {
            _toTime = value;
            UpdateDatadogUrl();
        }
    }
    

    protected override async Task OnInitializedAsync()
    {
        _loading = true;

        SelectedSeverities = _severities;
        SelectedServices = new List<string>();
        SelectedSources = new List<string>();
        
        StateHasChanged();
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
    }
    
    private void UpdateDatadogUrl()
    {
        _datadogUrl = GenerateDatadogUrl();
        StateHasChanged();
    }
    
    private MarkupString GetFormattedUrl()
    {
        if (string.IsNullOrEmpty(_datadogUrl))
            return new MarkupString("");
        
        var paramParts = _datadogUrl.Split('&');
        var formattedUrl = string.Join("<br>&", paramParts);
        
        var breakPoints = new[] { "+source", "+service", "+status" };
        foreach (var breakPoint in breakPoints)
        {
            var termParts = formattedUrl.Split(breakPoint);
            formattedUrl = string.Join("<br>" + breakPoint, termParts);
        }
        return new MarkupString(formattedUrl);
    }

    private async Task OpenDatadog()
    {
        await JSRuntime.InvokeVoidAsync("open", _datadogUrl, "_blank");
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
