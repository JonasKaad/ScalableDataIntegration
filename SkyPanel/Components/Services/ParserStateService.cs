using Microsoft.AspNetCore.Components;

namespace SkyPanel.Components.Services;
using SkyPanel.Components.Models;

public class ParserStateService
{ 
    private Parser? _parser;
    public string ParserName => _parser?.Name ?? string.Empty;
    
    public string Url => _parser?.Url ?? string.Empty;
    
    public int Polling => _parser?.Polling ?? 0;
    
    public string Protocol => _parser?.Protocol ?? string.Empty;

    public event Action? OnChange;
    
    public void SetParser(Parser? parser)
    {
        _parser = parser;
        NotifyStateChanged();
    }
    private void NotifyStateChanged() => OnChange?.Invoke();

    public bool ParserIsNotSelected() => _parser == null;
    
}