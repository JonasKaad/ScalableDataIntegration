namespace Downloader.Utils;

public class DisHttpClient : IDownloaderClient
{
    private HttpClient Client { get; }
    private string _url;
    
    public DisHttpClient(string url, string token = "", string tokenName = "")
    {
        // Create a new HttpClient
        Client = new HttpClient();
        
        // Set url and token
        _url = url;
        SetTokenHeader(token, tokenName);
    }


    public async Task<byte[]> FetchData()
    {
        if (Client == null)
        {
            throw new NullReferenceException("Client is null");
        }
        byte[] jsonResponse = [];
        try
        {
            using HttpResponseMessage response = await Client.GetAsync(_url);
            WriteRequestToConsole(response.EnsureSuccessStatusCode());
            
            jsonResponse = await response.Content.ReadAsByteArrayAsync();
            Console.WriteLine($"{jsonResponse}\n");
        }
        catch (HttpRequestException e)
        {
            //TODO: handle exception
            Console.WriteLine(e);
        }

        return jsonResponse;
    }

    public void SwitchHost(string url, string token = "", string tokenName = "")
    {
        // Clear headers
        Client.DefaultRequestHeaders.Clear();
        
        // Update client to new url and token
        _url = url;
        SetTokenHeader(token, tokenName);
    }

    public void Dispose()
    {
        Client.Dispose();
    }

    void WriteRequestToConsole(HttpResponseMessage response)
    { 
        var request = response.RequestMessage;
        Console.Write($"{request?.Method} ");
        Console.Write($"{request?.RequestUri} ");
        Console.WriteLine($"HTTP/{request?.Version}");
    }
    
    private void SetTokenHeader(string token, string tokenName)
    {
        if (!String.IsNullOrEmpty(token) && !String.IsNullOrEmpty(tokenName))
        {
            Client.DefaultRequestHeaders.Add(tokenName, token);
        }
    }
}