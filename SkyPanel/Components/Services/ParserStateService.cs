using Microsoft.AspNetCore.Components;

namespace SkyPanel.Components.Services;
using SkyPanel.Components.Models;

public class ParserStateService
{ 
    private Parser? _parser;
    public string ParserName => _parser?.Name ?? string.Empty;
    
    public string DownloadUrl => _parser?.DownloadUrl ?? string.Empty;
    public string BackupUrl => _parser?.BackupUrl ?? string.Empty;
    
    public string Polling => _parser?.PollingRate ?? string.Empty;
    
    public string ParserUrl => _parser?.ParserUrl ?? string.Empty;
    public string SecretName => _parser?.SecretName ?? string.Empty;

    public event Action? OnChange;
    
    public void SetParser(Parser? parser)
    {
        _parser = parser;
        NotifyStateChanged();
    }
    private void NotifyStateChanged() => OnChange?.Invoke();

    public bool ParserIsNotSelected() => _parser == null;
    
}