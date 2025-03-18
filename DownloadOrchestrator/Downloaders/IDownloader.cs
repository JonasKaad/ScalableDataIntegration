using Source = DownloadOrchestrator.Utils.IDownloaderClient.Source;

namespace DownloadOrchestrator.Downloaders;

public interface IDownloader
{
    public Task Download();
    public void SwitchSource(Source source, string url, string token = "", string tokenName = "");
    public void Log(string parserName, int bytesAmount, DateTime date);
    public string ToString();
}