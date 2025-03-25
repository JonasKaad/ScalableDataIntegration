using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Services;
using DownloadOrchestrator.Utils;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Sdi.Parser;

namespace Downloader.Downloaders;

public class BaseDownloaderJob : IDownloaderJob
{
    private readonly StatisticsContext _context;
    protected readonly SecretService SecretService;
    
    public BaseDownloaderJob() : this(new StatisticsContext(new DbContextOptionsBuilder<StatisticsContext>().Options), 
        new SecretService(new SecretClient(new Uri("uri"), new EnvironmentCredential())))
    {
    }
    public BaseDownloaderJob(StatisticsContext context, SecretService secretService)
    {
        _context = context; 
        SecretService = secretService;
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
                Console.WriteLine("Download Failed");
                return;
            }
            Log(data.Name, bytes.Length, DateTime.UtcNow);
            await SendToParser(bytes, data.ParserUrl);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
            Console.WriteLine($"Failed to download from {url}");
            return null;
        }
    }
    
    protected void Log(string parserName, int bytesAmount, DateTime date)
    {
        // Save to database
        Console.WriteLine(parserName + "," + bytesAmount + "," + date);
        var stats = new Dataset(){Parser = parserName, Date = date, DownloadedAmount = bytesAmount};
        _context.Add(stats);
        _context.SaveChanges();
    }

    public static async Task SendToParser(byte[] downloadedBytes, string parserUrl)
    {
        using var channel = GrpcChannel.ForAddress(parserUrl);
        var client = new Parser.ParserClient(channel);

        var reply = await client.ParseCallAsync(new (){ RawData = ByteString.CopyFrom(downloadedBytes), Format = "str"});
        Console.WriteLine($"Success: {reply.Success}. Msg: {reply.ErrMsg}");
    }
}