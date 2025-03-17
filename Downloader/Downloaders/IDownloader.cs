using Source = Downloader.Utils.IDownloaderClient.Source;

namespace Downloader.Downloaders;

public interface IDownloader
{
    public Task Download();
    public void SwitchSource(Source source, string url, string token = "", string tokenName = "");
    public void Log(string parserName, int bytesAmount, DateTime date);
}