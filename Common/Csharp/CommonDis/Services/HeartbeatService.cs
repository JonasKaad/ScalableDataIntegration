using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommonDis.Services;

public class HeartbeatService : BackgroundService
{
    private readonly ILogger<HeartbeatService> _logger;
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _heartbeatInterval;
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
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Heartbeat service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_heartbeatInterval, stoppingToken);
            try
            {
                var response = await _httpClient.PostAsync(_baseUrl + $"/{_parserName}/heartbeat", null, stoppingToken);
                _logger.LogInformation("Heartbeat sent. Status: {StatusCode}", response.StatusCode);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await RegisterParser(_httpClient,_baseUrl + $"/{_parserName}/register", _parserUrl, _logger);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending heartbeat");
            }
        }
    }

    public static async Task<bool> RegisterParser(HttpClient client, string url, string parserUrl, ILogger logger)
    {
        try
        {
       
            var response =      await client.PostAsJsonAsync(url, parserUrl);
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