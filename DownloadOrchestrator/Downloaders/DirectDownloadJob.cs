using Azure.Storage.Blobs;
using Downloader.Downloaders;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Utils;
using Microsoft.EntityFrameworkCore;

namespace DownloadOrchestrator.Downloaders;

public class DirectDownloadJob : BaseDownloaderJob
{
    public DirectDownloadJob() : base(new StatisticsContext(new DbContextOptionsBuilder<StatisticsContext>().Options))
    {
    }

    public DirectDownloadJob(StatisticsContext statisticsContext) : base(statisticsContext)
    {
    }

    public new async Task Download(DownloaderData data)
    {
        try
        {
            var bytes = await base.FetchBytes(data.DownloadUrl, data.TokenName, data.Token) 
                        ?? await base.FetchBytes(data.BackUpUrl, data.TokenName, data.Token);
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