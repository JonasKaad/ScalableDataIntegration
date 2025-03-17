using Downloader.Utils;
using Source = Downloader.Utils.IDownloaderClient.Source;
namespace Downloader.Utils;

public class DisDownloaderClient(string url = "", string token = "", string tokenName = "", Source source = Source.Http)
{
    private IDownloaderClient _downloaderClient = new DisHttpClient(url, token, tokenName);
    private Source _currentSource = source;

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
        var bytes = await _downloaderClient.FetchData();
        if (bytes.Length == 0)
        {
            throw new Exception($"Unable to fetch data from {_currentSource}");
        }
#if DEBUG
        Console.WriteLine($"Downloaded {bytes.Length} bytes");
#endif
        return bytes;
    }
}