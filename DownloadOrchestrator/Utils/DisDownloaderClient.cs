using Source = DownloadOrchestrator.Utils.IDownloaderClient.Source;
namespace DownloadOrchestrator.Utils;

public class DisDownloaderClient
{
    private IDownloaderClient _downloaderClient;
    private Source _currentSource;

    public DisDownloaderClient(string url = "", string token = "", string tokenName = "")
    {
        if (url[..3].Equals("ftp", StringComparison.OrdinalIgnoreCase))
        {
            CheckFtpCredentials(tokenName, token);
            _downloaderClient = new DisFtpClient(url, tokenName, token);
            _currentSource = Source.Ftp;
        }
        else if(url[..4].Equals("http", StringComparison.OrdinalIgnoreCase))
        {
            _downloaderClient = new DisHttpClient(url, token, tokenName);
            _currentSource = Source.Http;
        }
        else
        {
            throw new ArgumentException("Invalid URL. Are you missing the protocol?");
        }
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
                    CheckFtpCredentials(tokenName, token);
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
                    CheckFtpCredentials(tokenName, token);
                    _downloaderClient.SwitchHost(url, tokenName, token);
                    break;
                
                case Source.Http:
                    _downloaderClient.SwitchHost(url, tokenName, token);
                    break;
            }
        }
    }

    public override string ToString()
    {
        return _downloaderClient.ToString();
    }

    private static void CheckFtpCredentials(string tokenName, string token)
    {
        if (String.IsNullOrEmpty(token) || String.IsNullOrEmpty(tokenName))
        {
            // TODO: handle exception
            throw new ArgumentNullException(tokenName, "No credentials provided for FTP");
        }
    }
    
    public async Task<byte[]> FetchData()
    {
        var bytes = await _downloaderClient.FetchData();
        if (bytes.Length == 0)
        {
            throw new HttpRequestException($"Unable to fetch data from {_currentSource}");
        }
#if DEBUG
        Console.WriteLine($"Downloaded {bytes.Length} bytes");
#endif
        return bytes;
    }
}