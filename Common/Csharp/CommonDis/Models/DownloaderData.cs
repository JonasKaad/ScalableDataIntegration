namespace CommonDis.Models;

public class DownloaderData
{
    public string DownloadUrl { get; set; }
    public string BackUpUrl { get; set; }
    public string Parser { get; set; }
    public List<string> Filters { get; set; }
    public List<string> Parameters { get; set; }
    public string Name { get; set; }
    public string PollingRate { get; set; }
    public string SecretName { get; set; }

}