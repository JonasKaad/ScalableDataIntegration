using System.Text;
using System.Text.Json;
using SkyPanel.Components.Models;

namespace SkyPanel.Components.Services;

public sealed class OrchestratorClientService(IHttpClientFactory httpClientFactory, string baseUrl)
{
    public async Task<IEnumerable<string>> GetDownloaders()
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync($"{baseUrl}/downloaders");
            var elements = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
            return elements ?? [];
        } 
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return [];
    }

    public async Task<Parser> GetDownloaderConfiguration(string downloader)
    {
        var client = httpClientFactory.CreateClient();
        
        try
        {
            var response = await client.GetAsync($"{baseUrl}/{downloader}/configuration");
            var parser = await response.Content.ReadFromJsonAsync<Parser>();
            return parser ?? new Parser("", "", "", "", "", "");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return new Parser("", "", "", "", "", "");
    }

    public async Task ConfigureDownloader(string parser, string url, string backupUrl, string secretName, string pollingRate)
    {
        var client = httpClientFactory.CreateClient();

        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new 
            {
                name = parser,
                downloadUrl = url ?? " ",
                parserUrl = " ",
                backupUrl = backupUrl ?? " ",
                secretName = secretName ?? " ",
                pollingRate = pollingRate ?? " ",
            }),
            Encoding.UTF8,
            "application/json");
        using HttpResponseMessage response  = await client.PutAsync($"{baseUrl}/{parser}/configure", jsonContent);
        Console.WriteLine(response.StatusCode);
        
        var jsonResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"{jsonResponse}\n");
    }
    
    public async Task<List<bool>> TestConnection(string _parser, string _url, string _backupUrl, string _secretName, string _pollingRate)
    {
        Console.WriteLine($"Testing connection to {_parser} with URL: {_url} and backup URL: {_backupUrl}");
        var client = httpClientFactory.CreateClient();

        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new 
            {
                name = _parser,
                downloadUrl = _url,
                parserUrl = "",
                backupUrl = _backupUrl,
                secretName = _secretName,
                pollingRate = _pollingRate,
            }),
            Encoding.UTF8,
            "application/json");
        using HttpResponseMessage response  = await client.PostAsync($"{baseUrl}/test", jsonContent);
        Console.WriteLine(response.StatusCode);
        
        var jsonResponse = await response.Content.ReadFromJsonAsync<List<bool>>();
        if (jsonResponse is null)
        {
            Console.WriteLine("No response from the server");
            return [];
        }

        Console.WriteLine(jsonResponse.Count);
        Console.WriteLine(!string.IsNullOrEmpty(_backupUrl)
            ? $"Main URL: {jsonResponse.FirstOrDefault()} and Backup URL {jsonResponse.LastOrDefault()}\n"
            : $"Response: {jsonResponse.FirstOrDefault()}\n");

        return jsonResponse;
    }
}