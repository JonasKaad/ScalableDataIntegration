namespace DownloadOrchestrator.Models;

public class DownloaderData
{
    public string DownloadUrl { get; set; }
    public string BackUpUrl { get; set; }
    public string ParserUrl { get; set; }
    public string Name { get; set; }
    public string PollingRate { get; set; }

}