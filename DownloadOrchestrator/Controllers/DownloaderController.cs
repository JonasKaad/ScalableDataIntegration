using DownloadOrchestrator.Models;
using DownloadOrchestrator.Services;
using Microsoft.AspNetCore.Mvc;
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
    public ActionResult ConfigureDownloader(string downloader, string url = "", string backupUrl = "", string secretName = "", string pollingRate = "")
    {
        var dlToConfigure = _downloaders.FirstOrDefault(d => d.Name.Equals(downloader));
        if (dlToConfigure is null)
        {
            return NotFound("The downloader could not be found.");
        }
        
        try
        {
            dlToConfigure.DownloadUrl = string.IsNullOrEmpty(url) ? dlToConfigure.DownloadUrl : url;
            dlToConfigure.BackUpUrl = string.IsNullOrEmpty(backupUrl) ? dlToConfigure.BackUpUrl : backupUrl;
            dlToConfigure.PollingRate = string.IsNullOrEmpty(pollingRate) ? dlToConfigure.PollingRate : pollingRate;
            dlToConfigure.SecretName = string.IsNullOrEmpty(secretName) ? dlToConfigure.SecretName : secretName;
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
    public ActionResult Add(string downloader, string url, string backupUrl = "", string parser = "", string secretName = "", string pollingRate = "")
    {
        if (_downloaders.Any(d => d.Name.Equals(downloader)))
        {
            return BadRequest("The downloader already exists.");
        }
        
        var dl = new DownloaderData
        {
            DownloadUrl = url,
            BackUpUrl = backupUrl,
            ParserUrl = parser,
            Name = downloader,
            PollingRate = pollingRate,
            SecretName = secretName
        };
        
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
    
}