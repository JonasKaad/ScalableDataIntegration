using Downloader.Downloaders;
using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using static DownloadOrchestrator.Utils.IDownloaderClient;

namespace DownloadOrchestrator.Controllers;

[ApiController]
public class DownloaderController : ControllerBase
{
    private readonly PooledDbContextFactory<StatisticsContext> _dbContextFactory;
    private readonly List<IDownloader> _downloaders;

    public DownloaderController(PooledDbContextFactory<StatisticsContext> dbContextFactory, List<IDownloader> downloaders)
    {
        _dbContextFactory = dbContextFactory;
        _downloaders = downloaders;
    }

    [Route("downloaders")]
    [HttpGet]
    public ActionResult<List<string>> GetDownloaders()
    {
        return _downloaders.Select(dl => dl.ToString()).ToList();
    }

    [Route("{downloader}/configure")]
    [HttpPut]
    public ActionResult ConfigureDownloader(string downloader, string source, string url = "", string token = "", string tokenName = "")
    {
        var dlToConfigure = _downloaders.FirstOrDefault(d => d.ToString().Equals(downloader));
        if (dlToConfigure is null)
        {
            return NotFound("The downloader could not be found.");
        }

        var sourceType = GetSourceType(source);
        if (sourceType is null)
        {
            return BadRequest("Invalid source type");
        }

        try
        {
            dlToConfigure.SwitchSource(sourceType.Value, url, token, tokenName);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("An error occured while configuring the downloader.");
        }
        return Ok("Downloader has been updated.");
    }

    [Route("{downloader}/add")]
    [HttpPost]
    public ActionResult Add(string downloader, string source, string url, string parser, string token = "", string tokenName = "")
    {
        if (_downloaders.Any(d => d.ToString().Equals(downloader)))
        {
            return BadRequest("The downloader already exists.");
        }
        
        var sourceType = GetSourceType(source);
        if (sourceType is null)
        {
            return BadRequest("Invalid source type");
        }
        
        var client = new DisDownloaderClient(url, token, tokenName, sourceType.Value);
        var dl = new BaseDownloader(client, parser, downloader, _dbContextFactory);
        
        _downloaders.Add(dl);
        return Ok($"Downloader {downloader} has been added.");
    }

    [Route("{downloader}/reparse")]
    [HttpPost]
    public ActionResult Reparse(string downloader)
    {
        var dlToDownload = _downloaders.FirstOrDefault(d => d.ToString().Equals(downloader));
        if (dlToDownload is null)
        {
            return NotFound("The downloader could not be found.");
        }

        dlToDownload.Download();
        return Ok($"Downloader {downloader} has been started.");
    }
    

    private Source? GetSourceType(string source)
    {
        return source.ToUpper() switch
        {
            "HTTP" => Source.Http,
            "FTP" => Source.Ftp,
            _ => null
        };
    }
    
}