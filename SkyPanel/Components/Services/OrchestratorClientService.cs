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
            Console.WriteLine(parser);
            return parser ?? new Parser("", "", "", "", "", "");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return new Parser("", "", "", "", "", "");
    }
}