namespace Sdi.Parser;

public class DISHttpClient
{
    private HttpClient Client { get; }
    private Uri _url;
    
    public DISHttpClient(string address)
    {
        Client = new HttpClient();
        _url = new Uri(address);
    }
    
    public void SetUrl(string address)
    {
        _url = new Uri(address);
    }
    
    public async Task FetchData()
    {
        using HttpResponseMessage response = await Client.GetAsync(_url);
        WriteRequestToConsole(response.EnsureSuccessStatusCode());
    
        var jsonResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"{jsonResponse}\n");
    }
    
    void WriteRequestToConsole(HttpResponseMessage response)
    {
        if (response is null)
        {
            return;
        }

        var request = response.RequestMessage;
        Console.Write($"{request?.Method} ");
        Console.Write($"{request?.RequestUri} ");
        Console.WriteLine($"HTTP/{request?.Version}");        
    }
}