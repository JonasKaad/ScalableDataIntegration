using Downloader.Utils;
using Google.Protobuf;
using Grpc.Net.Client;
using Sdi.Parser;
using Source = Downloader.Utils.IDownloaderClient.Source;

namespace Downloader.Downloaders;

public class BaseDownloader : IDownloader
{
    private readonly DisDownloaderClient _downloaderClient;
    private readonly String _parser;

    public BaseDownloader(DisDownloaderClient downloaderClient, String parser)
    {
        _downloaderClient = downloaderClient ?? throw new ArgumentNullException(nameof(downloaderClient));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    public async Task Download()
    {
        var bytes = await _downloaderClient.FetchData();
        await SendToParser(bytes);
    }

    public void SwitchSource(Source source, string url, string token = "", string tokenName = "")
    {
        _downloaderClient.SwitchSource(source, url, token, tokenName);
    }

    private async Task SendToParser(byte[] downloadedBytes)
    {
        using var channel = GrpcChannel.ForAddress(_parser);
        var client = new Parser.ParserClient(channel);

        var reply = await client.ParseCallAsync(new (){ RawData = ByteString.CopyFrom(downloadedBytes) });
        Console.WriteLine($"Success: {reply.Success}. Msg: {reply.ErrMsg}");
    }
}