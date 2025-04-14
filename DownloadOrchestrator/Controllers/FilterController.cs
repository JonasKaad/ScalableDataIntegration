using CommonDis.Models;
using DownloadOrchestrator.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownloadOrchestrator.Controllers;

[Route("[controller]")]
[ApiController]
public class FilterController : ControllerBase
{
    private readonly ILogger<FilterController> _logger;
    private readonly FilterRegistry _filterRegistry;

    public FilterController(ILogger<FilterController> logger, FilterRegistry filterRegistry)
    {
        _logger = logger;
        _filterRegistry = filterRegistry;
    }

    [Route("filters")]
    [HttpGet]
    public ActionResult<List<string>> GetFilters()
    {
        return _filterRegistry.GetServices();
    }

    [Route("{filter}")]
    [HttpGet]
    public ActionResult<FilterDto> GetFilterData(string filter)
    {
        var data = _filterRegistry.GetService(filter);
        if (data is null)
        {
            return BadRequest("Filter not found");
        }
        return data;
    }

    [Route("{filter}/register")]
    [HttpPost]
    public ActionResult RegisterFilter(string filter, [FromBody] FilterData settings)
    {
        _logger.LogInformation("{Service} with {Parameters} at {Url} registered", settings.Name, settings.Parameters, settings.Url);
        _filterRegistry.RegisterService(filter, settings);
        return Ok($"Filter {filter} has been registered.");
    }

    [Route("{filter}/deregister")]
    [HttpDelete]
    public ActionResult DeregisterFilter(string filter)
    {
        _filterRegistry.DeRegisterService(filter);
        return Ok($"Filter {filter} has been deregistered.");
    }

    [Route("{filter}/heartbeat")]
    [HttpPost]
    public ActionResult FilterHeartbeat(string filter)
    {
        try
        {
            _filterRegistry.RefreshService(filter);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound($"{filter} has not been registered.");
        }
        return Ok($"Heartbeat Acknowledged for {filter}");
    }
}