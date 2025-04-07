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

    [Route("{filter}/register")]
    [HttpPost]
    public ActionResult RegisterFilter(string filter, [FromBody] string url)
    {
        _filterRegistry.RegisterService(filter, url);
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