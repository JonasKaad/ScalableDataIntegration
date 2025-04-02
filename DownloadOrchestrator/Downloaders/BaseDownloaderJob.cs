using CommonDis.Models;
using CommonDis.Services;
using DownloadOrchestrator.Utils;
using Google.Protobuf;
using Grpc.Net.Client;
using Sdi.Parser;

namespace DownloadOrchestrator.Downloaders;

public class BaseDownloaderJob : IDownloaderJob
{
    private readonly StatisticsDatabaseService _context;
    protected readonly SecretService SecretService;
    private readonly ILogger<IDownloaderJob> _logger;
    
    public BaseDownloaderJob(ILogger<BaseDownloaderJob> logger, StatisticsDatabaseService context, SecretService secretService)
    {
        _context = context; 
        SecretService = secretService;
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
            await SendToParser(bytes, data.Parser);
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

    public static async Task SendToParser(byte[] downloadedBytes, string parserUrl)
    {
        using var channel = GrpcChannel.ForAddress(parserUrl);
        var client = new Parser.ParserClient(channel);

        var reply = await client.ParseCallAsync(new (){ RawData = ByteString.CopyFrom(downloadedBytes), Format = "str"});
    }
}