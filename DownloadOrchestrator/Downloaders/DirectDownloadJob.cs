using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Services;
using Microsoft.EntityFrameworkCore;

namespace DownloadOrchestrator.Downloaders;

public class DirectDownloadJob : BaseDownloaderJob
{
    public DirectDownloadJob() : base(new StatisticsContext(new DbContextOptionsBuilder<StatisticsContext>().Options), 
        new SecretService(new SecretClient(new Uri("uri"), new EnvironmentCredential())))
    {
    }

    public DirectDownloadJob(StatisticsContext statisticsContext, SecretService secretService) : base(statisticsContext, secretService)
    {
    }

    public new async Task Download(DownloaderData data)
    {
        try
        {
            var secret = string.IsNullOrEmpty(data.SecretName) ? data.Name : data.SecretName;
            var downloaderSecret = await SecretService.GetSecretAsync(secret);
            var tokenName = downloaderSecret.TokenName;
            var token = downloaderSecret.Token;
            var bytes = await FetchBytes(data.DownloadUrl, tokenName, token) 
                        ?? await FetchBytes(data.BackUpUrl, tokenName, token);
            if (bytes is null)
            {
                Console.WriteLine("Download Failed");
                return;
            }
            Log(data.Name, bytes.Length, DateTime.UtcNow);
            await SendToParser(bytes, data.Name);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task SendToParser(byte[] downloadedBytes, string name)
    {
        var connectionString = Environment.GetEnvironmentVariable("blobConnection");
        var blobServiceClient = new BlobServiceClient(connectionString);

        var container = blobServiceClient.GetBlobContainerClient(name);
        await container.CreateIfNotExistsAsync();

        var date = DateTime.UtcNow.Date;
        var time = DateTime.UtcNow.ToString("HHmm");

        await container.UploadBlobAsync($"{date:yyyy/MM/dd}/{time}-direct_raw.txt", new BinaryData(downloadedBytes));
    }
}