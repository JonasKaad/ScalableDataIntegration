using Downloader.Downloaders;
using Downloader.Utils;
using Source = Downloader.Utils.IDownloaderClient.Source;

Dictionary<string, string> downloaders = new()
{
    { "1", "HTTP" },
    { "2", "FTP" },
    { "3", "Exit" },
};

var parsers = new Dictionary<string, string>()
{
    { "1", "http://csharpparser.jonaskaad.com" },
    { "2", "https://pythonparser.jonaskaad.com" },
};

var combinedClient = new DisDownloaderClient("http://sdihttp.jonaskaad.com");
var downloaderParser = parsers["1"];
var downloader = new BaseDownloader(combinedClient, downloaderParser);

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

    try
    {
        SetUrl(address, downloader);
        await downloader.Download();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }

}

void SetUrl(string protocol, BaseDownloader baseDownloader)
{
    Console.WriteLine("Enter URL:");
    var url = Console.ReadLine() ?? "";
    if (String.IsNullOrEmpty(url))
    {
        throw new ArgumentException("URL is required");
    }
    Console.WriteLine($"Enter {(String.Equals(protocol, "HTTP") ? "token name":"username")}:");
    var user = Console.ReadLine() ?? "";
    Console.WriteLine($"Enter {(String.Equals(protocol, "HTTP") ? "token":"password")}:");
    var password = Console.ReadLine() ?? "";

    if (protocol is not ("HTTP" or "FTP"))
    {
        throw new FormatException("How did you get here?");
    }
    
    switch (protocol)
    {
        case "HTTP":
            baseDownloader.SwitchSource(Source.Http, url, user, password);
            break;
        case "FTP":
            baseDownloader.SwitchSource(Source.Ftp, url, user, password);
            break;
    }
}
