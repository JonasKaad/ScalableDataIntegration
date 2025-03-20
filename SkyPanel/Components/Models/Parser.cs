using MudBlazor;

namespace SkyPanel.Components.Models;

public class Parser
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string BackupUrl { get; set; }
    public string Protocol { get; set; }
    public int Polling { get; set; }
    
    
    public Parser(string name, string url, string protocol, int polling, string backupUrl = "")
    {
        Name = name;
        Url = url;
        BackupUrl = backupUrl;
        Protocol = protocol;
        Polling = polling;
    }
}