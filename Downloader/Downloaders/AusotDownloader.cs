using System.Text;
using Downloader.Utils;

namespace Downloader.Downloaders;

public class AusotDownloader : BaseDownloader
{
    private DisDownloaderClient _downloaderClient;
    private readonly string _name = "AusotDownloader"; 
    public AusotDownloader(DisDownloaderClient downloaderClient, string parser) : base(downloaderClient, parser)
    {
        _downloaderClient = downloaderClient;
    }

    public new async Task Download()
    {
        try
        {
            Console.WriteLine("Downloading AusotTracks...");
            var bytes = await _downloaderClient.FetchData();

            Log(_name, bytes.Length, DateTime.Now);
            Console.WriteLine("Sending to parser...");
            await SendToParser(bytes);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}