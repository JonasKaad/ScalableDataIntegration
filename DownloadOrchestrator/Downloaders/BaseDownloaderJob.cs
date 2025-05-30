using CommonDis.Models;
using CommonDis.Services;
using DownloadOrchestrator.Services;
using DownloadOrchestrator.Utils;
using FluentFTP.Helpers;
using Google.Protobuf;
using Grpc.Net.Client;
using Sdi.Filter;
using Sdi.Parser;

namespace DownloadOrchestrator.Downloaders;

public class BaseDownloaderJob : IDownloaderJob
{
    private readonly StatisticsDatabaseService _context;
    protected readonly SecretService SecretService;
    protected readonly FilterRegistry _filterRegistry;
    protected readonly ParserRegistry _parserRegistry;
    private readonly ILogger<IDownloaderJob> _logger;
    protected readonly CommonService _commonService;
    
    public BaseDownloaderJob(ILogger<BaseDownloaderJob> logger, StatisticsDatabaseService context, SecretService secretService, ParserRegistry parserRegistry, FilterRegistry filterRegistry, CommonService commonService)
    {
        _context = context; 
        SecretService = secretService;
        _filterRegistry = filterRegistry;
        _parserRegistry = parserRegistry;
        _logger = logger;
        _commonService = commonService;
    }

    public virtual async Task Download(DownloaderData data)
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
            await Log(data.Name, bytes, DateTime.UtcNow);
            var parameters = data.Filters.Select(f => System.Text.Json.JsonSerializer.Serialize(f.Parameters)).ToList();
            var filterNames = data.Filters.Select(f => f.Name).ToList();
            var urls = filterNames.Select(filter => _filterRegistry.GetFilterUrl(filter)).ToList();
            var parserUrl = _parserRegistry.GetService(data.Parser);
            if(!string.IsNullOrEmpty(parserUrl))
                urls.Add(parserUrl);
            await SendToParser(bytes, urls, parameters);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error downloading from {Url} or {BackUpUrl} using {Secret} ", data.DownloadUrl, data.BackUpUrl, data.SecretName);
        }
    }

    protected async Task<byte[]?> FetchBytes(string url, string tokenName = "", string token = "")
    {
        if(string.IsNullOrEmpty(url)) return null;
        try
        {
            var client = new DisDownloaderClient(url, token, tokenName);
            var bytes = await client.FetchData();
            return bytes;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Failed to download from {Url}", url);
            return null;
        }
    }
    
    protected async Task Log(string parserName, byte[] bytes, DateTime date)
    {
        // Save to database
        var stream = new MemoryStream(bytes);
        await _commonService.SaveDataToBlob(parserName, stream);
        var bytesAmount = bytes.Length;
        _logger.LogInformation("Downloaded {Bytes} bytes for {Parser} on {Date}", bytesAmount, parserName, date);
        var stats = new Dataset(){Parser = parserName, Date = date, DownloadedAmount = bytesAmount};
        _context.Add(stats);
        await _context.SaveChangesAsync();
    }

    public static async Task SendToParser(byte[] downloadedBytes, List<string> urls, List<string> parameters)
    {
        var hasFilter = (urls.Count > 1);
        var address = urls.FirstOrDefault()!;
        using var channel = GrpcChannel.ForAddress(address);

        if (hasFilter)
        {
            var client = new Filter.FilterClient(channel);
            if (urls.Count > 2)
            {
                var reply = await client.FilterCallAsync(new ()
                {
                    RawData = ByteString.CopyFrom(downloadedBytes), 
                    Format = "img", 
                    Parameters = parameters.Join(";"),
                    NextUrls = urls.Skip(1).ToList().Join(";")
                });
            }
            else
            {
                var reply = await client.FilterCallAsync(new ()
                {
                    RawData = ByteString.CopyFrom(downloadedBytes), 
                    Format = "str", 
                    Parameters = parameters.Join(";"),
                    NextUrls = urls.Skip(1).ToList().Join(";")
                });
            }
        }
        else
        {
            var client = new Parser.ParserClient(channel);
            var reply = await client.ParseCallAsync(new (){ RawData = ByteString.CopyFrom(downloadedBytes), Format = "str"});
        }
    }
}