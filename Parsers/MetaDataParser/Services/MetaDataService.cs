using System.Net.Mime;
using Azure;
using Azure.Storage.Blobs;
using CommonDis.Services;
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
    private readonly CommonService _commonService;

    public MetaDataService(ILogger<MetaDataService> logger, CommonService commonService)
    {
        _logger = logger;
        _commonService = commonService;
    }

    public override async Task<ParseResponse> ParseCall(ParseRequest request, ServerCallContext context)
    {
        var data = GetBytes(request.RawData.ToByteArray());
        var text = System.Text.Encoding.Default.GetString(data.Last());
        var image = Image.Load(data.First());
        var newImage = Image.Load(data.First());
        newImage.Metadata.GetPngMetadata().TextData = new List<PngTextData>() {new PngTextData("valid", text, "en", "")};
        var ogImageSaved = new MemoryStream();
        await image.SaveAsync(ogImageSaved, PngFormat.Instance);
        ogImageSaved.Position = 0;
        var newImageSaved = new MemoryStream();
        await newImage.SaveAsync(newImageSaved, PngFormat.Instance);
        newImageSaved.Position = 0;
        
        await _commonService.SaveDataToBlob(_name, ogImageSaved, newImageSaved, "png");
        
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
    
        var startIndex = 0;
        int position;
    
        ReadOnlySpan<byte> dataSpan = data;
        ReadOnlySpan<byte> delimiterSpan = delimiter;
    
        while (startIndex <= data.Length - delimiter.Length)
        {
            position = dataSpan[startIndex..].IndexOf(delimiterSpan);
        
            if (position < 0)
                break; // No more delimiters found
            
            // Convert position to be relative to the original array
            position += startIndex + delimiter.Length - 1;
        
            if (position > startIndex) // Only add non-empty segments
            {
                var segmentSize = position - startIndex - delimiter.Length;
                var segment = new byte[segmentSize];
                Array.Copy(data, startIndex, segment, 0, segmentSize);
                result.Add(segment);
            }
        
            startIndex = position;
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
}