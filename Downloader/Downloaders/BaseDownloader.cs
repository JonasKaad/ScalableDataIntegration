using Downloader.Utils;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Sdi.Parser;
using Sdi.Parser.Models;
using Source = Downloader.Utils.IDownloaderClient.Source;

namespace Downloader.Downloaders;

public class BaseDownloader : IDownloader
{
    private readonly DisDownloaderClient _downloaderClient;
    private readonly string _parser;
    private readonly string _name;
    private readonly PooledDbContextFactory<StatisticsContext> _dbContextFactory;

    public BaseDownloader(DisDownloaderClient downloaderClient, string parser, string name = "basedownloader", PooledDbContextFactory<StatisticsContext> dbContextFactory = null)
    {
        _downloaderClient = downloaderClient ?? throw new ArgumentNullException(nameof(downloaderClient));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _name = name;
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task Download()
    {
        try
        {
            var bytes = await _downloaderClient.FetchData();
            Log(_name, bytes.Length, DateTime.UtcNow);
            await SendToParser(bytes);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void SwitchSource(Source source, string url, string token = "", string tokenName = "")
    {
        _downloaderClient.SwitchSource(source, url, token, tokenName);
    }

    public void Log(string parserName, int bytesAmount, DateTime date)
    {
        // Save to database
        Console.WriteLine(parserName + "," + bytesAmount + "," + date);
        var context = _dbContextFactory.CreateDbContext();
        var stats = new Dataset(){Parser = parserName, Date = date, DownloadedAmount = bytesAmount};
        context.Add(stats);
        context.SaveChanges();
    }
    
    protected async Task SendToParser(byte[] downloadedBytes)
    {
        using var channel = GrpcChannel.ForAddress(_parser);
        var client = new Parser.ParserClient(channel);

        var reply = await client.ParseCallAsync(new (){ RawData = ByteString.CopyFrom(downloadedBytes) });
        Console.WriteLine($"Success: {reply.Success}. Msg: {reply.ErrMsg}");
    }
}