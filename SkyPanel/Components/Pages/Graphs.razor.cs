using ApexCharts;
using CommonDis.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SkyPanel.Components.Pages;

public partial class Graphs
{
    private bool _loading = true;
    private ApexChart<ParserData> Chart { get; set; } = new();
    private ApexChartOptions<ParserData>? _options;
    [CascadingParameter] private bool IsDarkMode { get; set; }
    private IEnumerable<string> _selectedParsers = [];
    private List<string> _selected = [];
    private Dictionary<string, List<ParserData>> _parserData = new();
    private Dictionary<string, List<ParserData>> _rawParserData = new();
    private readonly List<string> _parsers = [];
    private DateRange Date { get; set; } = new(DateTime.Today, DateTime.Today);
    private TimeSpan? _fromTime = new TimeSpan(00, 00, 00);
    private TimeSpan? _toTime = new TimeSpan(23, 59, 59);
    private TimeSpan _groupSpan = new TimeSpan(1, 0, 0);
    private Amount _yaxis = Amount.Byte;
    [Inject] private StatisticsDatabaseService context { get; set; } = null!;

    protected override void OnInitialized()
    {
        _options = new ApexChartOptions<ParserData>
        {
            Yaxis =
            [
                new()
                {
                    Title = new() { Text = "bytes" },
                    Labels = new YAxisLabels()
                    {
                        Formatter = "(value) => Math.round(value * 10000) / 10000"
                    }
                }
            ],
            Chart = new Chart()
            {
                Stacked = true
            },
            PlotOptions = new PlotOptions
            {
                Bar = new PlotOptionsBar
                {
                    DataLabels = new PlotOptionsBarDataLabels
                    {
                        Total = new BarTotalDataLabels
                        {
                            Style = new BarDataLabelsStyle
                            {
                                FontWeight = "800"
                            },
                            Formatter = "(value) => Math.round(value * 10000) / 10000"
                        }
                    }
                }
            },
            Theme = new Theme
            {
                Mode = IsDarkMode ? Mode.Dark : Mode.Light,
                Palette = PaletteType.Palette1
            }
        };
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            LoadDataFromDb();
            _loading = false;
            StateHasChanged();
        }
    }
    private void LoadDataFromDb()
    {
        _loading = true;
        var start = DateTime.UtcNow - TimeSpan.FromDays(14);
        try
        {
            // Clear existing data
            _rawParserData.Clear();
            _parsers.Clear();
        
            FetchDataFromDb(start);
        
            // If there are selected parsers, update their data
            if (!_selectedParsers.Any()) return;
            foreach (var parser in _selectedParsers.ToList().Where(parser => !_rawParserData.ContainsKey(parser)))
            {
                _selected.Remove(parser);
                _selectedParsers = _selected;
            }
            if (_selected.Count != 0)
            {
                ClampData(Date.Start.GetValueOrDefault().Add(_fromTime.GetValueOrDefault()), 
                    Date.End.GetValueOrDefault().Add(_toTime.GetValueOrDefault()));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data from db: {ex.Message}");
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }

    private void FetchDataFromDb(DateTime start)
    {
        var parsers = context.Datasets.Where(d => d.Date > start).GroupBy(d => d.Parser);
        foreach (var parser in parsers)
        {
            var groupedData = parser.Select(dataset => new ParserData(dataset.Date, dataset.DownloadedAmount)).ToList();
            _rawParserData[parser.Key] = groupedData;
            if (!_parsers.Contains(parser.Key)) _parsers.Add(parser.Key);
        }
    }
    
    

    private async Task AddParser(IEnumerable<string> values)
    {
        _loading = true;
        var selected = values.ToList();
        _selectedParsers = selected;

        _selected = selected;

        await Zoom();
        _loading = false;
    }

    private void ClampData(DateTime start, DateTime end)
    {
        var ratio = 1;
        var axis = Chart.Options.Yaxis.FirstOrDefault();
        if (axis is not null)
        {
            var title = new AxisTitle();
            switch (_yaxis)
            {
                case Amount.Giga:
                    title.Text = "Gigabytes";
                    ratio = 1_000_000_000;
                    break;
                case Amount.Kilo:
                    title.Text = "Kilobytes";
                    ratio = 1_000;
                    break;
                case Amount.Mega:
                    title.Text = "Megabytes";
                    ratio = 1_000_000;
                    break;
                case Amount.Byte:
                    title.Text = "Bytes";
                    ratio = 1;
                    break;
            }

            axis.Title = title;
        }

        foreach (var parser in _selectedParsers)
        {
            _parserData[parser] = _rawParserData[parser].Where(p => start < p.Time && p.Time < end).ToList()
                .ConvertAll(p => new ParserData(p.Time, p.DownloadedBytes));
            foreach (var data in _parserData[parser])
            {
                data.Time = FloorDate(data.Time, _groupSpan);
                data.DownloadedBytes = Math.Round(data.DownloadedBytes / ratio, 4);
            }
        }
    }

    private DateTime FloorDate(DateTime date, TimeSpan span)
    {
        return date.AddTicks(-(date.Ticks % span.Ticks));
    }

    private async Task Zoom()
    {
        var start = Date.Start.GetValueOrDefault().Add(_fromTime.GetValueOrDefault());
        var end = Date.End.GetValueOrDefault().Add(_toTime.GetValueOrDefault());
        ClampData(start, end);
        await Chart.UpdateOptionsAsync(true, false, false);
    }

    public class ParserData(DateTime time, decimal downloadedBytes)
    {
        public DateTime Time { get; set; } = time;
        public decimal DownloadedBytes { get; set; } = downloadedBytes;
    }

    private enum Amount
    {
        Kilo,
        Mega,
        Giga,
        Byte
    }
}