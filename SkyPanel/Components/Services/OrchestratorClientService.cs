using System.Net;
using System.Text;
using System.Text.Json;
using SkyPanel.Components.Models;
using SkyPanel.Components.Models.Auth0;

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

    public async Task<bool> ConfigureDownloader(string parser, string url, string backupUrl, string secretName, string pollingRate)
    {
        
        var client = httpClientFactory.CreateClient();
        
        try
        {
            using StringContent jsonContent = new(
                JsonSerializer.Serialize(new
                {
                    name = parser,
                    downloadUrl = url ?? "",
                    parserUrl = "",
                    backupUrl = backupUrl ?? "",
                    secretName = secretName ?? "",
                    pollingRate = pollingRate ?? "",
                }),
                Encoding.UTF8,
                "application/json");
            using HttpResponseMessage response = await client.PutAsync($"{baseUrl}/{parser}/configure", jsonContent);
            var returnStatusCode = response.StatusCode;
            return returnStatusCode == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return false;
    }
    
    public async Task<List<bool>> TestConnection(string _parser, string _url, string _backupUrl, string _secretName, string _pollingRate)
    {
        var client = httpClientFactory.CreateClient();
        try
        {
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
            
            var jsonResponse = await response.Content.ReadFromJsonAsync<List<bool>>();
            return jsonResponse ?? [];
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return [];
    }

    public async Task<List<User>> GetUsers()
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync($"{baseUrl}/users");
            var users = await response.Content.ReadFromJsonAsync<List<User>>();
            return users ?? [];
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return [];
    }
    
    public async Task<List<Role>> GetUserRoles(string userId)
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync($"{baseUrl}/users/{userId}/roles");
            var roles = await response.Content.ReadFromJsonAsync<List<Role>>();
            return roles ?? [];
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return [];
    }
    
    public async Task<bool> RemoveUserRole(string userId, RoleData roles)
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/users/{userId}/roles")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { roles = roles.roles }),
                    Encoding.UTF8,
                    "application/json")
            };
            using HttpResponseMessage response = await client.SendAsync(request);
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
    public async Task<bool> UpdateUserRoles(string userId, RoleData roles )
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            using StringContent jsonContent = new(
                JsonSerializer.Serialize(new
                {
                    roles = roles.roles,
                }),
                Encoding.UTF8,
                "application/json");
                using HttpResponseMessage response = await client.PostAsync($"{baseUrl}/users/{userId}/roles", jsonContent);
                var returnStatusCode = response.StatusCode;
                return returnStatusCode == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return false;
    }
}