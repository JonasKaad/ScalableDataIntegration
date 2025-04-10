using CommonDis.Models;
using DownloadOrchestrator.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownloadOrchestrator.Controllers;

[Route("[controller]")]
[ApiController]
public class ParserController : ControllerBase
{
    private readonly ILogger<ParserController> _logger;
    private readonly ParserRegistry _parserRegistry;

    public ParserController(ILogger<ParserController> logger, ParserRegistry parserRegistry)
    {
        _logger = logger;
        _parserRegistry = parserRegistry;
    }

    [Route("parsers")]
    [HttpGet]
    public ActionResult<List<string>> GetParsers()
    {
        return _parserRegistry.GetServices();
    }

    [Route("{parser}/register")]
    [HttpPost]
    public ActionResult RegisterParser(string parser, [FromBody] ParserModel model)
    {
        _parserRegistry.RegisterService(parser, model.Url);
        return Ok($"Parser {parser} has been registered.");
    }

    [Route("{parser}/deregister")]
    [HttpDelete]
    public ActionResult DeregisterParser(string parser)
    {
        _parserRegistry.DeRegisterService(parser);
        return Ok($"Parser {parser} has been deregistered.");
    }

    [Route("{parser}/heartbeat")]
    [HttpPost]
    public ActionResult ParserHeartbeat(string parser)
    {
        try
        {
            _parserRegistry.RefreshService(parser);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound($"{parser} has not been registered.");
        }
        return Ok($"Heartbeat Acknowledged for {parser}");
    }
}