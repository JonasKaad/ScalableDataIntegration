using MudBlazor;

namespace SkyPanel.Components.Models;

public class Parser
{
    public string Name { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Url { get; set; }
    public string Protocol { get; set; }
    public int Polling { get; set; }
    
    
    public Parser(string name, string username, string password, string url, string protocol, int polling)
    {
        Name = name;
        Username = username;
        Password = password;
        Url = url;
        Protocol = protocol;
        Polling = polling;
    }
}