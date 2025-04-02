using System.Globalization;
using System.Text.RegularExpressions;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using CommonDis.Services;
using Grpc.Core;
using HtmlAgilityPack;
using Sdi.Parser;

namespace AusotParser.Services;

public class AusotParserService : Parser.ParserBase
{
    private static Regex AusRegex { get; } = new (@"TDM TRK ([A-Z0-9]{1,4}) ([0-9]{10})([0-9]+)[ A-Z]*\s*\r*\n*([0-9]*) ([0-9]*)\s*\r*\n*([0-9A-Z \r^\n^]+) - ([A-Z0-9 \r\n]*)RMK\s*\/\s*([0-9A-Z ]*)");

    private readonly ILogger<AusotParserService> _logger;
    private static string _name = "ausotparser";
    private CommonService _commonService;

    public AusotParserService(ILogger<AusotParserService> logger, CommonService common)
    {
        _logger = logger;
        _commonService = common;
    }

    public override async Task<ParseResponse> ParseCall(ParseRequest request, ServerCallContext context)
    {
        var doc = new HtmlDocument();
        doc.Load(new MemoryStream(request.RawData.ToByteArray()));
        var result = doc.DocumentNode.InnerText.Trim();
        var matches = AusRegex.Matches(result);

        if (matches.Count < 3)
        {
            _logger.LogWarning("Failed to match enough tracks, only matched {Matches}", matches);
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
        
        await _commonService.SaveDataToBlob(_name, new BinaryData(result), new BinaryData(tracks));
        _logger.LogInformation("Parsed {Tracks}", tracks.Count);

        return await Task.FromResult(new ParseResponse
        {
            Success = true,
            ErrMsg = $"Parsed {tracks.Count} tracks"
        });
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