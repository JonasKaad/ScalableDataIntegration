using Downloader.Utils;
using FluentFTP;

namespace Sdi.Parser.Utils;

public class DisFtpClient : IDownloaderClient
{
    private AsyncFtpClient _ftpClient;
    
    public DisFtpClient(string host, string userName, string password)
    {
        _ftpClient = new AsyncFtpClient(host, userName, password);
    }

    public async Task FetchData()
    {
        await _ftpClient.Connect();
        var firstItem = _ftpClient.GetListing().Result.Skip(1).FirstOrDefault();
        await DownloadFile(firstItem?.Name);
    }

    public void SwitchHost(string host, string username, string password)
    {
        if (_ftpClient.IsConnected)
        {
            _ftpClient.Disconnect();
        }
        _ftpClient.Host = host;
        _ftpClient.Credentials.UserName = username;
        _ftpClient.Credentials.Password = password;
    }

    public void Dispose()
    {
        _ftpClient.Dispose();
    }

    public async Task DownloadFile(string sourceFile)
    {
        //await _ftpClient.Connect();
        if (await _ftpClient.FileExists(sourceFile))
        {
            var downloadedBytes = await _ftpClient.DownloadBytes(sourceFile, CancellationToken.None);
            Console.WriteLine(downloadedBytes.Length);
        }
        else
        {
            foreach (var listing in await _ftpClient.GetListing())
            {
                Console.WriteLine(listing);   
            }
            throw new FileNotFoundException($"File not found at {_ftpClient.Host}:", sourceFile);
        }
        await _ftpClient.Disconnect();
    }
}