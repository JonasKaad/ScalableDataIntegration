using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace CommonDis.Services;

public class CommonService
{
    private readonly ILogger<CommonService> _logger;

    public CommonService(ILogger<CommonService> logger)
    {
        _logger = logger;
    }

    public async Task SaveDataToBlob(string parser, BinaryData raw, BinaryData parsed, string format = "txt")
    {
        var connectionString = Environment.GetEnvironmentVariable("blobConnection");
        var blobServiceClient = new BlobServiceClient(connectionString);

        var container = blobServiceClient.GetBlobContainerClient(parser);
        await container.CreateIfNotExistsAsync();

        var date = DateTime.UtcNow.Date;
        var hour = DateTime.UtcNow.Hour;
        var min = DateTime.UtcNow.Minute;

        try
        {
            await container.UploadBlobAsync($"{date:yyyy/MM/dd}/{hour}{min}-tracks_raw.{format}", raw);
            await container.UploadBlobAsync($"{date:yyyy/MM/dd}/{hour}{min}-tracks_parsed.{format}", parsed);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError("Failed to upload to blob: {Error}", ex);
        }
    }
    
    public async Task SaveDataToBlob(string parser, Stream raw, Stream parsed, string format = "txt")
    {
        var connectionString = Environment.GetEnvironmentVariable("blobConnection");
        var blobServiceClient = new BlobServiceClient(connectionString);

        var container = blobServiceClient.GetBlobContainerClient(parser);
        await container.CreateIfNotExistsAsync();

        var date = DateTime.UtcNow.Date;
        var hour = DateTime.UtcNow.Hour;
        var min = DateTime.UtcNow.Minute;

        try
        {
            await container.UploadBlobAsync($"{date:yyyy/MM/dd}/{hour}{min}-tracks_raw.{format}", raw);
            await container.UploadBlobAsync($"{date:yyyy/MM/dd}/{hour}{min}-tracks_parsed.{format}", parsed);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError("Failed to upload to blob: {Error}", ex);
        }
    }
}

