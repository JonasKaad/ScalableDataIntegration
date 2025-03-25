namespace DownloadOrchestrator.Utils;

public interface IDownloaderClient
{
    public Task<byte[]> FetchData();
    public void SwitchHost(string host, string name, string password);
    public void Dispose();
    public string ToString();
    public Task<bool> CanConnect();
     public enum Source
    {
        Ftp,
        Http
    }
}