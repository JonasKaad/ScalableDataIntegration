using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Utils;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Sdi.Parser;

namespace Downloader.Downloaders;

public class BaseDownloaderJob : IDownloaderJob
{
    private readonly StatisticsContext _context;
    
    public BaseDownloaderJob() : this(new StatisticsContext(new DbContextOptionsBuilder<StatisticsContext>().Options))
    {
    }
    public BaseDownloaderJob(StatisticsContext context)
    {
        _context = context;
    }

    public virtual async Task Download(DownloaderData data)
    {
        try
        {
            var client = new DisDownloaderClient(data.DownloadUrl, data.Token, data.TokenName);
            var bytes = await client.FetchData();
            Log(data.Name, bytes.Length, DateTime.UtcNow);
            await SendToParser(bytes, data.ParserUrl);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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

    private async Task SendToParser(byte[] downloadedBytes, string parserUrl)
    {
        using var channel = GrpcChannel.ForAddress(parserUrl);
        var client = new Parser.ParserClient(channel);

        var reply = await client.ParseCallAsync(new (){ RawData = ByteString.CopyFrom(downloadedBytes) });
        Console.WriteLine($"Success: {reply.Success}. Msg: {reply.ErrMsg}");
    }
}