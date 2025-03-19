using Downloader.Downloaders;
using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Services;
using DownloadOrchestrator.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using static DownloadOrchestrator.Utils.IDownloaderClient;

namespace DownloadOrchestrator.Controllers;

[ApiController]
public class DownloaderController : ControllerBase
{
    private readonly IDownloaderService _downloaderService;
    private readonly List<DownloaderData> _downloaders;

    public DownloaderController(IDownloaderService downloaderService, List<DownloaderData> downloaders)
    {
        _downloaderService = downloaderService;
        _downloaders = downloaders;
    }

    [Route("downloaders")]
    [HttpGet]
    public ActionResult<List<string>> GetDownloaders()
    {
        return _downloaders.Select(dl => dl.Name).ToList();
    }
    
    [Route("{downloader}/configuration")]
    [HttpGet]
    public ActionResult<DownloaderData> GetDownloaderConfiguration(string downloader)
    {
        var dlToConfigure = _downloaders.FirstOrDefault(d => d.Name.Equals(downloader));
        if (dlToConfigure is null)
        {
            return NotFound("The downloader could not be found.");
        }

        return dlToConfigure;
    }
    

    [Route("{downloader}/configure")]
    [HttpPut]
    public ActionResult ConfigureDownloader(string downloader, string source, string url = "", string token = "", string tokenName = "")
    {
        var dlToConfigure = _downloaders.FirstOrDefault(d => d.Name.Equals(downloader));
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
            dlToConfigure.DownloadUrl = url;
            dlToConfigure.Token = token;
            dlToConfigure.TokenName = tokenName;
            _downloaderService.ScheduleOrUpdateRecurringDownload(dlToConfigure);
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
        if (_downloaders.Any(d => d.Name.Equals(downloader)))
        {
            return BadRequest("The downloader already exists.");
        }
        
        var sourceType = GetSourceType(source);
        if (sourceType is null)
        {
            return BadRequest("Invalid source type");
        }
        
        var dl = new DownloaderData{DownloadUrl = url, TokenName = tokenName, Token = token, ParserUrl = parser, Name = downloader};
        
        _downloaders.Add(dl);
        _downloaderService.ScheduleOrUpdateRecurringDownload(dl);
        return Ok($"Downloader {downloader} has been added.");
    }

    [Route("{downloader}/reparse")]
    [HttpPost]
    public ActionResult Reparse(string downloader)
    {
        var dlToDownload = _downloaders.FirstOrDefault(d => d.Name.Equals(downloader));
        if (dlToDownload is null)
        {
            return NotFound("The downloader could not be found.");
        }

        _downloaderService.ScheduleDownload(dlToDownload);
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