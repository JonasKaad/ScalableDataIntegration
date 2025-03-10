using Downloader.Utils;
using Google.Protobuf;
using Grpc.Net.Client;
using Sdi.Parser;
using Source = Downloader.Utils.IDownloaderClient.Source;

Dictionary<string, string> downloaders = new()
{
    { "1", "HTTP" },
    { "2", "FTP" },
    { "3", "Exit" },
};

var parsers = new Dictionary<string, string>()
{
    { "1", "http://newdis-parser--tdixdfe.agreeablewave-0f8fa504.westeurope.azurecontainerapps.io" },
    { "2", "https://python-test--rku67b7.agreeablewave-0f8fa504.westeurope.azurecontainerapps.io" },
};

var combinedClient = new DisDownloaderClient();

while (true)
{
    Console.WriteLine("Select a Downloader:\n1 - HTTP URL\n2 - FTP URL\n3 - Exit");
    var selectedDownloader = Console.ReadLine() ?? "";

    if (!downloaders.TryGetValue(selectedDownloader, out var address))
    {
        Console.WriteLine("Invalid downloader selected");
        throw new ArgumentException($"Unknown source type: {selectedDownloader}");
    }

    if (address == "Exit")
    {
        return;
    }
    var downloadedBytes = await FetchData(address, combinedClient);
    if (downloadedBytes.Length == 0)
    {
        throw new Exception($"Unable to fetch data from {address}");
    }
    Console.WriteLine($"Downloaded {downloadedBytes.Length} bytes");
    Console.WriteLine("Select a parser:\n1 - csharp-parser\n2 - python-parser\n3 - testingparser");

    var selectedParser = Console.ReadLine() ?? "";

    if (!parsers.TryGetValue(selectedParser, out var parser))
    {
        Console.WriteLine("Invalid downloader selected");
        throw new ArgumentException($"Unknown source type: {selectedParser}");
    }

    using var channel = GrpcChannel.ForAddress(parser);
    var client = new Parser.ParserClient(channel);

    var reply = await client.ParseCallAsync(new (){ RawData = ByteString.CopyFrom(downloadedBytes) });
    Console.WriteLine($"Success: {reply.Success}. Msg: {reply.ErrMsg}");
}

async Task<byte[]> FetchData(string value, DisDownloaderClient disDownloaderClient)
{
    Console.WriteLine("Enter URL:");
    var url = Console.ReadLine() ?? "";
    if (String.IsNullOrEmpty(url))
    {
        throw new ArgumentException("URL is required");
    }
    Console.WriteLine("Enter user/tokenName:");
    var user = Console.ReadLine() ?? "";
    Console.WriteLine("Enter password:");
    var password = Console.ReadLine() ?? "";

    if (value is not ("HTTP" or "FTP"))
    {
        throw new FormatException("How did you get here?");
    }

    switch (value)
    {
        case "HTTP":
            disDownloaderClient.SwitchSource(Source.Http, url, user, password);
            break;
        case "FTP":
            disDownloaderClient.SwitchSource(Source.Ftp, url, user, password);
            break;
    }

    return await disDownloaderClient.FetchData();
}
