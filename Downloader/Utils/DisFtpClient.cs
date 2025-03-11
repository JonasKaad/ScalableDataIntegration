using FluentFTP;

namespace Downloader.Utils;

public class DisFtpClient : IDownloaderClient
{
    private AsyncFtpClient _ftpClient;
    
    public DisFtpClient(string host, string userName, string password)
    {
        int port = 21;
        if (host[5..].Contains(':'))
        {
            string[] parts = host.Split(':');
            host = parts[0] + ":" + parts[1];
            port = int.Parse(parts[2]);
        }

        _ftpClient = new AsyncFtpClient(host, userName, password, port);
    }

    public async Task<byte[]> FetchData()
    {
        await _ftpClient.Connect();
        var firstItem = _ftpClient.GetListing().Result.Skip(1).FirstOrDefault();
        return await DownloadFile(firstItem?.Name);
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

    private async Task<byte[]> DownloadFile(string sourceFile)
    {
        byte[] downloadedBytes;
        if (await _ftpClient.FileExists(sourceFile))
        {
            downloadedBytes = await _ftpClient.DownloadBytes(sourceFile, CancellationToken.None);
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
        return downloadedBytes;
    }
}