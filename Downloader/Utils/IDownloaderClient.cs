namespace Downloader.Utils;

public interface IDownloaderClient
{
    public Task FetchData();
    public void SwitchHost(string host, string name, string password);

    public void Dispose();
    
     public enum Source
    {
        Ftp,
        Http
    }
}