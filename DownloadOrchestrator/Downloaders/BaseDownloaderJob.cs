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
    private readonly ILogger<IDownloaderJob> _logger;
    
    public BaseDownloaderJob(ILogger<BaseDownloaderJob> logger, StatisticsDatabaseService context, SecretService secretService, FilterRegistry filterRegistry)
    {
        _context = context; 
        SecretService = secretService;
        _filterRegistry = filterRegistry;
        _logger = logger;
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
            Log(data.Name, bytes.Length, DateTime.UtcNow);
            var parameters = data.Filters.Select(f => string.Join(",", f.Parameters.Select(p => $"{{'{p.Key}':'{p.Value}'}}"))).ToList();
            var filterNames = data.Filters.Select(f => f.Name).ToList();
            var urls = filterNames.Select(filter => _filterRegistry.GetFilterUrl(filter)).ToList();
            urls.Add(data.Parser);
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
    
    protected void Log(string parserName, int bytesAmount, DateTime date)
    {
        // Save to database
        _logger.LogInformation("Downloaded {Bytes} bytes for {Parser} on {Date}", bytesAmount, parserName, date);
        var stats = new Dataset(){Parser = parserName, Date = date, DownloadedAmount = bytesAmount};
        _context.Add(stats);
        _context.SaveChanges();
    }

    public static async Task SendToParser(byte[] downloadedBytes, List<string> urls, List<string> parameters)
    {
        var hasFilter = (urls.Count > 1);
        var address = urls.FirstOrDefault()!;
        using var channel = GrpcChannel.ForAddress(address);

        if (hasFilter)
        {
            var client = new Filter.FilterClient(channel);
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
            var client = new Parser.ParserClient(channel);
            var reply = await client.ParseCallAsync(new (){ RawData = ByteString.CopyFrom(downloadedBytes), Format = "str"});
        }
    }
}