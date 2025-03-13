namespace SkyPanel.Components.Services;
using SkyPanel.Components.Models;

public class ParserStateService
{
    private IDictionary<string, IDictionary<string, string>> _secretDictionary = new Dictionary<string, IDictionary<string, string>>();

    private Parser? _parser;
    
    public Parser Parser => _parser;
    public string ParserName => _parser?.Name ?? string.Empty;
    public string Username => _parser?.Username ?? string.Empty;
    public string Password => _parser?.Password ?? string.Empty;
    
    public string Url => _parser?.Url ?? string.Empty;
    
    public int Polling => _parser?.Polling ?? 0;
    
    public string Protocol => _parser?.Protocol ?? string.Empty;

    public event Action? OnChange;
    
    
    public void InitializeDictionary(IDictionary<string, IDictionary<string, string>> secretDictionary)
    {
        _secretDictionary = secretDictionary;
    }
    
    public void SetParser(Parser? parser)
    {
        _parser = parser;
        NotifyStateChanged();
    }
    private void NotifyStateChanged() => OnChange?.Invoke();

    
    public Parser[] TestParsers =
    [
        new("NatTrackParser", "jonas", "1234", "https://www.google.com", "Http", 24),
        new("BufrParser", "victor", "4321", "ftp://www.test.com", "Ftp", 37)
    ];
}