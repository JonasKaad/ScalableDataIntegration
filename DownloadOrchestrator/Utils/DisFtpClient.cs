using FluentFTP;

namespace DownloadOrchestrator.Utils;
public class DisFtpClient : IDownloaderClient
{
    private AsyncFtpClient _ftpClient;
    
    public DisFtpClient(string host, string userName, string password)
    {
        if (host.Length < "ftp://j.dk".Length)
        {
            throw new ArgumentException("Hostname is too short, did you forget the protocol?");
        }
        int port = 21;
        bool useSftp = false;
        
        if (host.Contains("ftp://"))
        {
            string[] parts = host.Split(':');
            if (parts[0].Contains("sftp"))
            {
                useSftp = true;
            }
            host = parts[1][2..];
        }
        if (host.Contains(':'))
        {
            port = int.Parse(host.Split(":").Last());
        }

        Console.WriteLine(host);

        _ftpClient = new AsyncFtpClient(host, userName, password, port);
        if (useSftp)
        {
            _ftpClient.Config.EncryptionMode = FtpEncryptionMode.Explicit;
        }
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

    public override string ToString()
    {
        return $"Downloading using FTP from {_ftpClient.Host} using {_ftpClient.Credentials.UserName} " +
               $"and {_ftpClient.Credentials.Password} on port {_ftpClient.Port}";
    }
    
    public async Task<bool> CanConnect()
    {
        await _ftpClient.Connect();
        var canConnect = _ftpClient.IsConnected;
        await _ftpClient.Disconnect();
        return canConnect;
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