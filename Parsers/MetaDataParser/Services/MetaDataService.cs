using System.Net.Mime;
using Azure;
using Azure.Storage.Blobs;
using Grpc.Core;
using Microsoft.Extensions.Azure;
using Sdi.Parser;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using SixLabors.ImageSharp.Metadata;

namespace MetaDataParser.Services;

public class MetaDataService : Parser.ParserBase
{
    private readonly ILogger<MetaDataService> _logger;
    private readonly string _name = "metadataparser";

    public MetaDataService(ILogger<MetaDataService> logger)
    {
        _logger = logger;
    }

    public override async Task<ParseResponse> ParseCall(ParseRequest request, ServerCallContext context)
    {
        var data = GetBytes(request.RawData.ToByteArray());
        var text = System.Text.Encoding.Default.GetString(data.Last());
        var image = Image.Load(data.First());
        var newImage = Image.Load(data.First());
        newImage.Metadata.GetPngMetadata().TextData = new List<PngTextData>() {new PngTextData("valid", text, "en", "")};
        await SaveDataToBlob(image, newImage);
        
        return await Task.FromResult(new ParseResponse
        {
            Success = true,
            ErrMsg = $"Successfully injected metadata"
        });
    }
    
    private List<byte[]> GetBytes(byte[] data)
    {
        var delimiter = "magic"u8.ToArray();
        var result = new List<byte[]>();
    
        int startIndex = 0;
        int position;
    
        ReadOnlySpan<byte> dataSpan = data;
        ReadOnlySpan<byte> delimiterSpan = delimiter;
    
        while (startIndex <= data.Length - delimiter.Length)
        {
            position = dataSpan[startIndex..].IndexOf(delimiterSpan);
        
            if (position < 0)
                break; // No more delimiters found
            
            // Convert position to be relative to the original array
            position += startIndex;
        
            if (position > startIndex) // Only add non-empty segments
            {
                var segment = new byte[position - startIndex];
                Array.Copy(data, startIndex, segment, 0, position - startIndex);
                result.Add(segment);
            }
        
            startIndex = position + delimiter.Length;
        }
    
        // Add the last segment if there's data after the last delimiter
        if (startIndex < data.Length)
        {
            var lastSegment = new byte[data.Length - startIndex];
            Array.Copy(data, startIndex, lastSegment, 0, data.Length - startIndex);
            result.Add(lastSegment);
        }
    
        return result;
    }
    
    private async Task SaveDataToBlob(Image ogImg, Image img)
    {
        DotNetEnv.Env.Load();
        var ogImageSaved = new MemoryStream();
        await ogImg.SaveAsync(ogImageSaved, PngFormat.Instance);
        ogImageSaved.Position = 0;
        var newImageSaved = new MemoryStream();
        await img.SaveAsync(newImageSaved, PngFormat.Instance);
        newImageSaved.Position = 0;
        var connectionString = Environment.GetEnvironmentVariable("blobConnection");
        var blobServiceClient = new BlobServiceClient(connectionString);

        var container = blobServiceClient.GetBlobContainerClient(_name);
        await container.CreateIfNotExistsAsync();

        var date = DateTime.UtcNow.Date;
        var hour = DateTime.UtcNow.Hour;
        var min = DateTime.UtcNow.Minute;

        try
        {
            await container.UploadBlobAsync($"{date:yyyy/MM/dd}/{hour}{min}-tracks_raw.png", ogImageSaved);
            await container.UploadBlobAsync($"{date:yyyy/MM/dd}/{hour}{min}-tracks_parsed.png", newImageSaved);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError("Failed to upload to blob: {Error}", ex);
        }
    }
}