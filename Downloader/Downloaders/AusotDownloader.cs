using System.Text;
using Downloader.Utils;
using HtmlAgilityPack;

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
            var doc = new HtmlDocument();
            var bytes = await _downloaderClient.FetchData();
            doc.Load(new MemoryStream(bytes));
            var result = Encoding.UTF8.GetBytes(doc.DocumentNode.InnerText.Trim());

            Log(_name, result.Length, DateTime.Now);
            await SendToParser(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}