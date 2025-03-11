using Downloader.Utils;
using Source = Downloader.Utils.IDownloaderClient.Source;
namespace Downloader.Utils;

public class DisDownloaderClient
{
    IDownloaderClient _downloaderClient;
    Source _currentSource;
    
    public DisDownloaderClient()
    {
        _downloaderClient = new DisHttpClient("");
        _currentSource = Source.Http;
    }
    
    public void SwitchSource(Source source, string url, string tokenName = "", string  token = "")
    {
        if (_currentSource != source)
        {
            _currentSource = source;
            _downloaderClient.Dispose();
            switch (source)
            {
                case Source.Ftp:
                    CheckIfFtpCredentials(tokenName, token);
                    _downloaderClient = new DisFtpClient(url, tokenName, token);
                    break;
                
                case Source.Http:
                    _downloaderClient = new DisHttpClient(url, tokenName, token);
                    break;
            }
        }
        else
        {
            switch (source)
            {
                case Source.Ftp:
                    CheckIfFtpCredentials(tokenName, token);
                    _downloaderClient.SwitchHost(url, tokenName, token);
                    break;
                
                case Source.Http:
                    _downloaderClient.SwitchHost(url, tokenName, token);
                    break;
            }
        }
    }

    private static void CheckIfFtpCredentials(string tokenName, string token)
    {
        if (String.IsNullOrEmpty(token) || String.IsNullOrEmpty(tokenName))
        {
            // TODO: handle exception
            throw new ArgumentNullException("", "No credentials provided for FTP");
        }
    }
    
    public async Task<byte[]> FetchData()
    {
        return await _downloaderClient.FetchData();
    }
}