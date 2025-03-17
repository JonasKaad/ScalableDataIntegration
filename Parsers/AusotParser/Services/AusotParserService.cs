using System.Globalization;
using System.Text.RegularExpressions;
using Azure.Identity;
using Azure.Storage.Blobs;
using Grpc.Core;
using HtmlAgilityPack;
using Sdi.Parser;

namespace AusotParser.Services;

public class AusotParserService : Parser.ParserBase
{
    private static Regex AusRegex { get; } = new (@"TDM TRK ([A-Z0-9]{1,4}) ([0-9]{10})([0-9]+)[ A-Z]*\s*\r*\n*([0-9]*) ([0-9]*)\s*\r*\n*([0-9A-Z \r^\n^]+) - ([A-Z0-9 \r\n]*)RMK\s*\/\s*([0-9A-Z ]*)");

    private readonly ILogger<AusotParserService> _logger;
    private static string _name = "ausotparser";

    public AusotParserService(ILogger<AusotParserService> logger)
    {
        _logger = logger;
    }

    public override async Task<ParseResponse> ParseCall(ParseRequest request, ServerCallContext context)
    {
        var doc = new HtmlDocument();
        doc.Load(new MemoryStream(request.RawData.ToByteArray()));
        var result = doc.DocumentNode.InnerText.Trim();
        var matches = AusRegex.Matches(result);
        Console.WriteLine(result);

        if (matches.Count < 3)
        {
            return await Task.FromResult(new ParseResponse()
            {
                Success = false,
                ErrMsg = result.Length.ToString()
            });
        }

        var tracks = new List<Track>();
        foreach (var match in matches.Cast<Match>())
        {
            if (match.Groups.Count == 9)
            {
                var msg = match.Groups[0].Value;
                var id = match.Groups[1].Value;
                var createdAt = DateTime.ParseExact(match.Groups[2].Value, "yyMMddHHmm", CultureInfo.InvariantCulture);
                var number =int.Parse(match.Groups[3].Value);
                var validFrom = DateTime.ParseExact(match.Groups[4].Value, "yyMMddHHmm", CultureInfo.InvariantCulture);
                var validTo = DateTime.ParseExact(match.Groups[5].Value, "yyMMddHHmm", CultureInfo.InvariantCulture);
                var trackString = match.Groups[6].Value;
                var rts = match.Groups[7].Value;
                var comments = match.Groups[8].Value;
                var track = new Track(id, msg, createdAt, number, validFrom, validTo, trackString, rts, comments);
                tracks.Add(track);
            }
        }
        
        await SaveDataToBlob(result, tracks);

        return await Task.FromResult(new ParseResponse
        {
            Success = true,
            ErrMsg = $"Parsed {tracks.Count} tracks"
        });
    }

    private static async Task SaveDataToBlob(string result, List<Track> tracks)
    {
        var blobServiceClient = new BlobServiceClient(
            new Uri("https://parserstorage.blob.core.windows.net"),
            new DefaultAzureCredential());

        var container = blobServiceClient.GetBlobContainerClient(_name);
        await container.CreateIfNotExistsAsync();

        var date = DateTime.UtcNow.Date;
        var hour = DateTime.UtcNow.Hour;
        var min = DateTime.UtcNow.Minute;

        await container.UploadBlobAsync($"{date:yyyy/MM/dd}/{hour}{min}-tracks_raw.txt", new BinaryData(result));
        await container.UploadBlobAsync($"{date:yyyy/MM/dd}/{hour}{min}-tracks_parsed.txt", new BinaryData(tracks));
    }

    private class Track(
        string id,
        string msg,
        DateTime createdAt,
        int number,
        DateTime validFrom,
        DateTime validTo,
        string trackString,
        string rts,
        string comments)
    {
        private readonly string _id = id;
        private readonly string _msg = msg;
        private readonly DateTime _createdAt = createdAt;
        private readonly int _number = number;
        private readonly DateTime _validFrom = validFrom;
        private readonly DateTime _validTo = validTo;
        private readonly string _trackString = trackString;
        private readonly string _rts = rts;
        private readonly string _comments = comments;
    }
}