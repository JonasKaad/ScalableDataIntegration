namespace SkyPanel.Components.Services;

public sealed class OrchestratorClientService(IHttpClientFactory httpClientFactory, string baseUrl)
{
    public async Task<List<string>> GetDownloaders()
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync($"{baseUrl}downloaders");
        var elements = await response.Content.ReadFromJsonAsync<List<string>>();
        return elements;
    }
}