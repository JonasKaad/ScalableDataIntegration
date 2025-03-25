using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Services;
using DownloadOrchestrator.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DownloadOrchestrator.Controllers;

[ApiController]
public class DownloaderController : ControllerBase
{
    private readonly IDownloaderService _downloaderService;
    private readonly List<DownloaderData> _downloaders;
    private readonly SecretService _secretService;

    public DownloaderController(IDownloaderService downloaderService, List<DownloaderData> downloaders, SecretService secretService)
    {
        _downloaderService = downloaderService;
        _downloaders = downloaders;
        _secretService = secretService;
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
        var dlToConfigure = GetDownloader(downloader);
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
        var dlToConfigure = GetDownloader(downloader);
        if (dlToConfigure is null)
        {
            return NotFound("The downloader could not be found.");
        }
        
        try
        {
            dlToConfigure.ParserUrl = HandleConfiguration(dlToConfigure.ParserUrl, dlConfiguration.ParserUrl);
            dlToConfigure.DownloadUrl = HandleConfiguration(dlToConfigure.DownloadUrl, dlConfiguration.DownloadUrl);
            dlToConfigure.BackUpUrl = HandleConfiguration(dlToConfigure.BackUpUrl, dlConfiguration.BackUpUrl);
            dlToConfigure.PollingRate = HandleConfiguration(dlToConfigure.PollingRate, dlConfiguration.PollingRate);
            dlToConfigure.SecretName = HandleConfiguration(dlToConfigure.SecretName, dlConfiguration.SecretName);
            _downloaderService.ScheduleOrUpdateRecurringDownload(dlToConfigure);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("An error occured while configuring the downloader.");
        }
        return Ok("Downloader has been updated.");
    }

    private static string HandleConfiguration(string oldValue, string newValue)
    {
        var url = string.IsNullOrEmpty(newValue) ? oldValue : newValue.Trim();
        return url;
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

    [Route("/test")]
    [HttpPost]
    public async Task<ActionResult<List<bool>>> TestConnection(DownloaderData downloader)
    {
        var secret = new DisSecret();
        if (!string.IsNullOrWhiteSpace(downloader.SecretName))
        {
            secret = await _secretService.GetSecretAsync(downloader.SecretName);
        }
        var client = new DisDownloaderClient(downloader.DownloadUrl, secret.Token, secret.TokenName);
        var mainResult = await client.CanConnect();
        if (string.IsNullOrWhiteSpace(downloader.BackUpUrl)) return new List<bool> {mainResult};
        client.SwitchSource(downloader.BackUpUrl, secret.TokenName, secret.Token);
        var backUpResult = await client.CanConnect();
        return new List<bool> {mainResult, backUpResult};
    }
}
