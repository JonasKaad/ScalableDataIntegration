using System.Net.Http.Json;
using CommonDis.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommonDis.Services;

public class HeartbeatService : BackgroundService
{
    private readonly ILogger<HeartbeatService> _logger;
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _heartbeatInterval = TimeSpan.FromMinutes(5);
    private readonly string _baseUrl;
    private readonly string _parserUrl;
    private readonly string _parserName;

    public HeartbeatService(ILogger<HeartbeatService> logger, string baseUrl, string name, string parserUrl, TimeSpan interval)
    {
        _logger = logger;
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
        _parserName = name;
        _parserUrl = parserUrl;
        _heartbeatInterval = interval;
        _ = RegisterParser(_httpClient, _baseUrl + $"/Parser/{_parserName}/register", new() {Url = _parserUrl}, _logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Heartbeat service for {Name} starting", _parserName);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_heartbeatInterval, stoppingToken);
            try
            {
                var response = await _httpClient.PostAsync(_baseUrl + $"/Parser/{_parserName}/heartbeat", null, stoppingToken);
                _logger.LogInformation("Heartbeat sent. Status: {StatusCode}", response.StatusCode);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await RegisterParser(_httpClient,_baseUrl + $"/Parser/{_parserName}/register", new(){ Url = _parserUrl}, _logger);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending heartbeat");
            }
        }
    }

    public static async Task<bool> RegisterParser(HttpClient client, string url, ParserModel parserUrl, ILogger logger)
    {
        try
        {
       
            var response = await client.PostAsJsonAsync(url, parserUrl);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error registering parser");
        }

        return false;
    }

    public override void Dispose()
    {
        _httpClient.Dispose();
        base.Dispose();
    }
}