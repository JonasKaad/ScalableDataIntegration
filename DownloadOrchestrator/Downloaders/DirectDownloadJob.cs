using Azure.Storage.Blobs;
using CommonDis.Models;
using CommonDis.Services;

namespace DownloadOrchestrator.Downloaders;

public class DirectDownloadJob : BaseDownloaderJob
{
    private readonly ILogger<DirectDownloadJob> _logger;

    public DirectDownloadJob(ILogger<DirectDownloadJob> logger, StatisticsDatabaseService statisticsContext, SecretService secretService) : base(logger, statisticsContext, secretService)
    {
        _logger = logger;
    }

    public new async Task Download(DownloaderData data)
    {
        try
        {
            var secret = string.IsNullOrEmpty(data.SecretName) ? data.Name : data.SecretName;
            var downloaderSecret = await SecretService.GetSecretAsync(secret);
            var tokenName = downloaderSecret?.TokenName ?? "";
            var token = downloaderSecret?.Token ?? "";
            var bytes = await FetchBytes(data.DownloadUrl, tokenName, token) 
                        ?? await FetchBytes(data.BackUpUrl, tokenName, token);
            if (bytes is null)
            {
                _logger.LogError("Unable to fetch bytes from {Url} or {BackUpUrl}", data.DownloadUrl, data.BackUpUrl);
                return;
            }
            Log(data.Name, bytes.Length, DateTime.UtcNow);
            await SendToParser(bytes, data.Name);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error downloading from {Url} or {BackUpUrl} using {Secret} ", data.DownloadUrl, data.BackUpUrl, data.SecretName);
        }
    }

    private new async Task SendToParser(byte[] downloadedBytes, string name)
    {
        var connectionString = Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING");
        var blobServiceClient = new BlobServiceClient(connectionString);

        var container = blobServiceClient.GetBlobContainerClient(name);
        await container.CreateIfNotExistsAsync();

        var date = DateTime.UtcNow.Date;
        var time = DateTime.UtcNow.ToString("HHmm");
        
        _logger.LogInformation("Saving raw data to {Container} at {Date}-direct_raw.txt", name, $"{date:yyyy/MM/dd}/{time}");
        await container.UploadBlobAsync($"{date:yyyy/MM/dd}/{time}-direct_raw.txt", new BinaryData(downloadedBytes));
    }
}