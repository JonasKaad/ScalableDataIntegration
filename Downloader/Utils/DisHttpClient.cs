namespace Downloader.Utils;

public class DisHttpClient
{
    private HttpClient Client { get; }
    private string _url;
    
    public DisHttpClient(string url, string token = "", string tokenName = "")
    {
        // Create a new HttpClient
        Client = new HttpClient();
        
        // Set url and token
        _url = url;
        if (token != "" && tokenName != "")
        {
            Client.DefaultRequestHeaders.Add(tokenName, token);
        }
    }

    public async Task FetchData()
    {
        try
        {
            using HttpResponseMessage response = await Client.GetAsync(_url);
            WriteRequestToConsole(response.EnsureSuccessStatusCode());
            
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{jsonResponse}\n");
        }
        catch (HttpRequestException e)
        {
            //TODO: handle exception
            Console.WriteLine(e);
        }
    }

    }
    
    void WriteRequestToConsole(HttpResponseMessage response)
    { 
        var request = response.RequestMessage;
        Console.Write($"{request?.Method} ");
        Console.Write($"{request?.RequestUri} ");
        Console.WriteLine($"HTTP/{request?.Version}");
    }
}