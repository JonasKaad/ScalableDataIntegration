using ApexCharts;
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
    private readonly List<string> _parsers = ["parser", "2ndParser"];
    private DateRange Date { get; set; } = new(DateTime.Today, DateTime.Today);
    private TimeSpan? _fromTime = new TimeSpan(00, 00, 00);
    private TimeSpan? _toTime = new TimeSpan(23, 59, 59);
    private TimeSpan _groupSpan = new TimeSpan(1, 0, 0);
    private Amount _yaxis = Amount.Mega;

    protected override void OnInitialized()
    {
        _options = new ApexChartOptions<ParserData>
        {
            Yaxis =
            [
                new()
                {
                    Title = new() { Text = "bytes" }
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

        _rawParserData.Add(_parsers.First(), new() {
            new(time: new DateTime(2025, 03, 13, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 456),
            new(time: new DateTime(2025, 03, 13, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 789),
            new(time: new DateTime(2025, 03, 13, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 234),
            new(time: new DateTime(2025, 03, 13, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 567),
            new(time: new DateTime(2025, 03, 14, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 890),
            new(time: new DateTime(2025, 03, 14, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 123),
            new(time: new DateTime(2025, 03, 14, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 678),
            new(time: new DateTime(2025, 03, 14, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 345),
            new(time: new DateTime(2025, 03, 15, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 901),
            new(time: new DateTime(2025, 03, 15, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 234),
            new(time: new DateTime(2025, 03, 15, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 567),
            new(time: new DateTime(2025, 03, 15, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 890),
            new(time: new DateTime(2025, 03, 16, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 123),
            new(time: new DateTime(2025, 03, 16, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 456),
            new(time: new DateTime(2025, 03, 16, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 789),
            new(time: new DateTime(2025, 03, 16, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 234),
            new(time: new DateTime(2025, 03, 17, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 567),
            new(time: new DateTime(2025, 03, 17, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 890),
            new(time: new DateTime(2025, 03, 17, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 123),
            new(time: new DateTime(2025, 03, 17, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 456),
            new(time: new DateTime(2025, 03, 18, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 789),
            new(time: new DateTime(2025, 03, 18, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 234),
            new(time: new DateTime(2025, 03, 18, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 567),
            new(time: new DateTime(2025, 03, 18, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 890),
            new(time: new DateTime(2025, 03, 19, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 123),
            new(time: new DateTime(2025, 03, 19, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 456),
            new(time: new DateTime(2025, 03, 19, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 789),
            new(time: new DateTime(2025, 03, 19, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 234),
        });

        _rawParserData.Add(_parsers.Skip(1).First(), [
            new(time: new DateTime(2025, 03, 13, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 321),
            new(time: new DateTime(2025, 03, 13, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 654),
            new(time: new DateTime(2025, 03, 13, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 187),
            new(time: new DateTime(2025, 03, 13, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 923),
            new(time: new DateTime(2025, 03, 14, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 546),
            new(time: new DateTime(2025, 03, 14, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 789),
            new(time: new DateTime(2025, 03, 14, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 432),
            new(time: new DateTime(2025, 03, 14, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 101),
            new(time: new DateTime(2025, 03, 15, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 876),
            new(time: new DateTime(2025, 03, 15, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 543),
            new(time: new DateTime(2025, 03, 15, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 210),
            new(time: new DateTime(2025, 03, 15, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 765),
            new(time: new DateTime(2025, 03, 16, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 398),
            new(time: new DateTime(2025, 03, 16, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 621),
            new(time: new DateTime(2025, 03, 16, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 954),
            new(time: new DateTime(2025, 03, 16, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 287),
            new(time: new DateTime(2025, 03, 17, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 732),
            new(time: new DateTime(2025, 03, 17, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 165),
            new(time: new DateTime(2025, 03, 17, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 890),
            new(time: new DateTime(2025, 03, 17, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 543),
            new(time: new DateTime(2025, 03, 18, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 276),
            new(time: new DateTime(2025, 03, 18, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 709),
            new(time: new DateTime(2025, 03, 18, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 132),
            new(time: new DateTime(2025, 03, 18, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 865),
            new(time: new DateTime(2025, 03, 19, 00, 00, 00, DateTimeKind.Utc), downloadedBytes: 498),
            new(time: new DateTime(2025, 03, 19, 06, 00, 00, DateTimeKind.Utc), downloadedBytes: 321),
            new(time: new DateTime(2025, 03, 19, 12, 00, 00, DateTimeKind.Utc), downloadedBytes: 654),
            new(time: new DateTime(2025, 03, 19, 18, 00, 00, DateTimeKind.Utc), downloadedBytes: 987),
        ]);
        _loading = false;
        StateHasChanged();
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
                    ratio = 1000000;
                    break;
                case Amount.Kilo:
                    title.Text = "Kilobytes";
                    ratio = 1;
                    break;
                case Amount.Mega:
                    title.Text = "Megabytes";
                    ratio = 1000;
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
        Giga
    }
}