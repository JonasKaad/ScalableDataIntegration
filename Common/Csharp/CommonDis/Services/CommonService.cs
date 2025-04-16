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
        var container = await GetContainerClient(parser);

        var date = DateTime.UtcNow;

        try
        {
            await container.UploadBlobAsync($"{date:yyyy/MM/dd/HHmm}-raw.{format}", raw);
            await container.UploadBlobAsync($"{date:yyyy/MM/dd/HHmm}-parsed.{format}", parsed);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError("Failed to upload to blob: {Error}", ex);
        }
    }
    
    public async Task SaveDataToBlob(string parser, Stream raw, Stream parsed, string format = "txt")
    {
        var container = await GetContainerClient(parser);

        var date = DateTime.UtcNow;

        try
        {
            await container.UploadBlobAsync($"{date:yyyy/MM/dd/HHmm}-raw.{format}", raw);
            await container.UploadBlobAsync($"{date:yyyy/MM/dd/HHmm}-parsed.{format}", parsed);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError("Failed to upload to blob: {Error}", ex);
        }
    }

    private static async Task<BlobContainerClient> GetContainerClient(string parser)
    {
        var connectionString = Environment.GetEnvironmentVariable("BLOB_CONNECTION");
        var blobServiceClient = new BlobServiceClient(connectionString);

        var container = blobServiceClient.GetBlobContainerClient(parser);
        await container.CreateIfNotExistsAsync();
        return container;
    }
}

