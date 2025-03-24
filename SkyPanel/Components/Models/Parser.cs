using MudBlazor;

namespace SkyPanel.Components.Models;

public class Parser
{
    public string Name { get; set; }
    public string DownloadUrl { get; set; }
    public string BackupUrl { get; set; }
    public string PollingRate { get; set; }
    public string ParserUrl { get; set; }
    public string SecretName { get; set; }
    
    
    public Parser(string name, string downloadUrl, string parserUrl, string pollingRate, string backupUrl = "", string secretName = "")
    {
        Name = name;
        DownloadUrl = downloadUrl;
        BackupUrl = backupUrl;
        ParserUrl = parserUrl;
        PollingRate = pollingRate;
        SecretName = secretName;
    }
}