using Downloader.Utils;
using Google.Protobuf;
using Grpc.Net.Client;
using Sdi.Parser;
using Source = Downloader.Utils.IDownloaderClient.Source;

namespace Downloader.Downloaders;

public class BaseDownloader : IDownloader
{
    private readonly DisDownloaderClient _downloaderClient;
    private readonly string _parser;
    private readonly string _name;

    public BaseDownloader(DisDownloaderClient downloaderClient, string parser, string name = "basedownloader")
    {
        _downloaderClient = downloaderClient ?? throw new ArgumentNullException(nameof(downloaderClient));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _name = name;
    }

    public async Task Download()
    {
        try
        {
            var bytes = await _downloaderClient.FetchData();
            Log(_name, bytes.Length, DateTime.Now);
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
    }
    
    protected async Task SendToParser(byte[] downloadedBytes)
    {
        using var channel = GrpcChannel.ForAddress(_parser);
        var client = new Parser.ParserClient(channel);

        var reply = await client.ParseCallAsync(new (){ RawData = ByteString.CopyFrom(downloadedBytes) });
        Console.WriteLine($"Success: {reply.Success}. Msg: {reply.ErrMsg}");
    }
}