using MudBlazor;

namespace SkyPanel.Components.Models;

public class Parser
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string Protocol { get; set; }
    public int Polling { get; set; }
    
    
    public Parser(string name, string url, string protocol, int polling)
    {
        Name = name;
        Url = url;
        Protocol = protocol;
        Polling = polling;
    }
}