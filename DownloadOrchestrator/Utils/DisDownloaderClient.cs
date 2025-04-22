using Source = DownloadOrchestrator.Utils.IDownloaderClient.Source;
namespace DownloadOrchestrator.Utils;

public class DisDownloaderClient
{
    private IDownloaderClient _downloaderClient;
    private Source _currentSource;

    public DisDownloaderClient(string url = "", string token = "", string tokenName = "")
    {
        _currentSource = GetSourceFromUrl(url);
        SetNewClient(url, tokenName, token, _currentSource);
    }

    private Source GetSourceFromUrl(string url)
    {
        if (url[..4].Contains("ftp", StringComparison.OrdinalIgnoreCase))
        { 
            return Source.Ftp;
        }
        if(url[..4].Equals("http", StringComparison.OrdinalIgnoreCase))
        {
            return Source.Http;
        }
        throw new ArgumentException("Invalid URL. Are you missing the protocol?");
    }

    public void SwitchSource(string url, string tokenName = "", string  token = "")
    {
        var source = GetSourceFromUrl(url);
        if (_currentSource != source)
        {
            _currentSource = source;
            _downloaderClient.Dispose();
            SetNewClient(url, tokenName, token, source);
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

    private void SetNewClient(string url, string tokenName, string token, Source source)
    {
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

    public override string ToString()
    {
        return _downloaderClient.ToString();
    }

    private static void CheckFtpCredentials(string tokenName, string token)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(tokenName))
        {
            // TODO: handle exception
            throw new ArgumentNullException(tokenName, "No credentials provided for FTP");
        }
    }

    public async Task<bool> CanConnect()
    {
        return await _downloaderClient.CanConnect();
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