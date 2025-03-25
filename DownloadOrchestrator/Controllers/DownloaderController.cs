using System.Text;
using Downloader.Downloaders;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Services;
using Microsoft.AspNetCore.Mvc;

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
    public ActionResult ConfigureDownloader(string downloader, [FromBody] DownloaderData dlConfiguration)
    {
        var dlToConfigure = _downloaders.FirstOrDefault(d => d.Name.Equals(downloader));
        if (dlToConfigure is null)
        {
            return NotFound("The downloader could not be found.");
        }
        
        try
        {
            dlToConfigure.ParserUrl = string.IsNullOrEmpty(dlConfiguration.ParserUrl) ? dlToConfigure.ParserUrl : dlConfiguration.ParserUrl;
            dlToConfigure.DownloadUrl = string.IsNullOrEmpty(dlConfiguration.DownloadUrl) ? dlToConfigure.DownloadUrl : dlConfiguration.DownloadUrl;
            dlToConfigure.BackUpUrl = string.IsNullOrEmpty(dlConfiguration.BackUpUrl) ? dlToConfigure.BackUpUrl : dlConfiguration.BackUpUrl;
            dlToConfigure.PollingRate = string.IsNullOrEmpty(dlConfiguration.PollingRate) ? dlToConfigure.PollingRate : dlConfiguration.PollingRate;
            dlToConfigure.SecretName = string.IsNullOrEmpty(dlConfiguration.SecretName) ? dlToConfigure.SecretName : dlConfiguration.SecretName;
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
    public ActionResult Add(string downloader, [FromBody] DownloaderData newDl)
    {
        if (string.IsNullOrEmpty(newDl.Name))
        {
            return BadRequest("The downloader must have a name.");
        }
        if (_downloaders.Any(d => d.Name.Equals(downloader)))
        {
            return BadRequest("The downloader already exists.");
        }
        if(string.IsNullOrEmpty(newDl.DownloadUrl) && string.IsNullOrEmpty(newDl.BackUpUrl))
        {
            return BadRequest("The downloader must have a download url or a backup rate.");
        }
        if (string.IsNullOrEmpty(newDl.PollingRate))
        {
            return BadRequest("The downloader must have a polling rate.");
        }
        
        _downloaders.Add(newDl);
        _downloaderService.ScheduleOrUpdateRecurringDownload(newDl);
        return Ok($"Downloader {downloader} has been added.");
    }

    [Route("{downloader}/reparse")]
    [HttpPost]
    public ActionResult Reparse(string downloader)
    {
        var dlToDownload = GetDownloader(downloader);
        if (dlToDownload is null)
        {
            return NotFound("The downloader could not be found.");
        }

        _downloaderService.ScheduleDownload(dlToDownload);
        return Ok($"Downloader {downloader} has been started.");
    }

    [Route("{downloader}/upload")]
    [HttpPost]
    public async Task<ActionResult> Parse(string downloader, List<IFormFile> formFiles)
    {
        var dl = GetDownloader(downloader);
        if (dl is null)
        {
            return NotFound("The downloader could not be found.");
        }
        List<byte> totalBytes = new List<byte>();
        foreach (var file in formFiles)
        {
            await using var sr = file.OpenReadStream();
            byte[] bytes = new byte[sr.Length];
            await sr.ReadExactlyAsync(bytes, 0, bytes.Length);
            totalBytes.AddRange(bytes);
            totalBytes.AddRange("magic"u8.ToArray());
        }
        await BaseDownloaderJob.SendToParser(totalBytes.ToArray(), dl.ParserUrl);
        return Ok($"Parsing for {downloader} has been started with uploaded data.");
    }


    private DownloaderData? GetDownloader(string downloader) =>  _downloaders.FirstOrDefault(d => d.Name.Equals(downloader));
}
