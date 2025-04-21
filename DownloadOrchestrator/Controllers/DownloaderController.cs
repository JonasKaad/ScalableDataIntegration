using CommonDis.Models;
using CommonDis.Services;
using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Services;
using DownloadOrchestrator.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DownloadOrchestrator.Controllers;

[Route("[controller]")]
[ApiController]
public class DownloaderController : ControllerBase
{
    private readonly IDownloaderService _downloaderService;
    private readonly List<DownloaderData> _downloaders;
    private readonly SecretService _secretService;
    private readonly ILogger<DownloaderController> _logger;
    private readonly ParserRegistry _parserRegistry;
    private readonly FilterRegistry _filterRegistry;

    public DownloaderController(IDownloaderService downloaderService, SecretService secretService, 
        ILogger<DownloaderController> logger, ParserRegistry parserRegistry, FilterRegistry filterRegistry)
    {
        _downloaderService = downloaderService;
        _downloaders = downloaderService.GetRecurringJobs();
        _secretService = secretService;
        _logger = logger;
        _parserRegistry = parserRegistry;
        _filterRegistry = filterRegistry;
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
            _logger.LogInformation("Tried to find downloader {Downloader} but it was not found", downloader);
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
            var parser = !string.IsNullOrEmpty(dlConfiguration.Parser) ?  _parserRegistry.GetService(dlConfiguration.Parser.ToLowerInvariant()) : "";
            if(parser is null)
            {
                _logger.LogWarning("Tried to configure downloader {Downloader} with parser {Parser} but the parser was not found", downloader, dlConfiguration.Parser);
                return BadRequest("The parser could not be resolved.");
            }
            dlToConfigure.Parser = HandleConfiguration(dlToConfigure.Parser, parser!);


            var isRemovingFilters = dlConfiguration.Filters.Any(f => string.IsNullOrWhiteSpace(f.Name) || f.Parameters.Count == 0);
            var filters = dlConfiguration.Filters
                .Select(filter =>
                {
                    var temp = _filterRegistry.GetService(filter.Name.ToLowerInvariant());
                    if (temp is null) return null;
                    temp.Parameters = filter.Parameters;
                    return temp;
                })
                .ToList();
            if (dlConfiguration.Filters.Count != 0 && filters.Any(f => string.IsNullOrEmpty(f.Name)) && !isRemovingFilters)
            {
                _logger.LogWarning("Failed to set filters for downloader {Downloader} with filters {Filters}", downloader, dlConfiguration.Filters);
                return BadRequest("Some or more filters could not be resolved.");
            }

            if (filters.Count != 0 || isRemovingFilters)
            {
                dlToConfigure.Filters = filters!.Where(f => !string.IsNullOrEmpty(f.Name)).ToList();
            }
            
            dlToConfigure.DownloadUrl = HandleConfiguration(dlToConfigure.DownloadUrl, dlConfiguration.DownloadUrl);
            dlToConfigure.BackUpUrl = HandleConfiguration(dlToConfigure.BackUpUrl, dlConfiguration.BackUpUrl);
            dlToConfigure.PollingRate = HandleConfiguration(dlToConfigure.PollingRate, dlConfiguration.PollingRate);
            dlToConfigure.SecretName = HandleConfiguration(dlToConfigure.SecretName, dlConfiguration.SecretName);
            _downloaderService.ScheduleOrUpdateRecurringDownload(dlToConfigure);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error configuring downloader {Downloader} with {Configuration}", downloader, dlConfiguration);
            return BadRequest("An error occured while configuring the downloader.");
        }
        return Ok("Downloader has been updated.");
    }

    private static string HandleConfiguration(string oldValue, string newValue)
    {
        var url = string.IsNullOrEmpty(newValue) ? oldValue : newValue.Trim();
        return url;
    }

    [Route("add")]
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

        newDl.Parser = _parserRegistry.GetService(newDl.Parser.ToLowerInvariant()) ?? "";
        newDl.Filters = newDl.Filters.Select(filter =>
        {
            var registeredFilter = _filterRegistry.GetService(filter.Name.ToLowerInvariant());
            if (registeredFilter != null)
            {
                registeredFilter.Parameters = filter.Parameters;
            }
            return registeredFilter;
        }).ToList()!;
        
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
        var totalBytes = new List<byte>();
        foreach (var file in formFiles)
        {
            await using var sr = file.OpenReadStream();
            var bytes = new byte[sr.Length];
            await sr.ReadExactlyAsync(bytes, 0, bytes.Length);
            totalBytes.AddRange(bytes);
            if (formFiles.Count > 1)
            {
                totalBytes.AddRange("magic"u8.ToArray());
            }
        }

        var parameters = dl.Filters.Select(f => System.Text.Json.JsonSerializer.Serialize(f.Parameters)).ToList();
        var filterNames = dl.Filters.Select(f => f.Name).ToList();
        var urls = filterNames.Select(filter => _filterRegistry.GetFilterUrl(filter)).ToList();
        urls.Add(dl.Parser);
        await BaseDownloaderJob.SendToParser(totalBytes.ToArray(), urls, parameters);
        return Ok($"Parsing for {downloader} has been started with uploaded data.");
    }


    private DownloaderData? GetDownloader(string downloader) =>  _downloaders.FirstOrDefault(d => d.Name.Equals(downloader));

    [Route("test")]
    [HttpPost]
    public async Task<ActionResult<List<bool>>> TestConnection([FromBody]DownloaderData downloader)
    {
        var secret = new DisSecret();
        if (!string.IsNullOrWhiteSpace(downloader.SecretName))
        {
            secret = await _secretService.GetSecretAsync(downloader.SecretName);
        }
        var mainResult = false;
        var backUpResult = false;
        DisDownloaderClient? client = null;
        try
        {
            client = new DisDownloaderClient(downloader.DownloadUrl, secret.Token, secret.TokenName);
            mainResult = await client.CanConnect();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to connect to {Url} with {Secret} when testing connection", downloader.DownloadUrl, downloader.SecretName);
        }

        try
        {
            if (string.IsNullOrWhiteSpace(downloader.BackUpUrl))
            {
                return new List<bool>{mainResult};
            }
            if (client is null)
            {
                client = new DisDownloaderClient(downloader.BackUpUrl, secret.Token, secret.TokenName);
            }
            else
            {
                client.SwitchSource(downloader.BackUpUrl, secret.TokenName, secret.Token);
            }

            backUpResult = await client.CanConnect();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e,"Failed to connect to {Url} with {Secret} when testing connection", downloader.DownloadUrl, downloader.SecretName);
        }
        return new List<bool> {mainResult, backUpResult};
    }
}
