using System.Net;
using System.Text;
using System.Text.Json;
using CommonDis.Models;
using CommonDis.Models.Auth0;
using SkyPanel.Components.Models;
namespace SkyPanel.Components.Services;

public sealed class OrchestratorClientService(IHttpClientFactory httpClientFactory, string baseUrl, ILogger<OrchestratorClientService> logger)
{
    public async Task<IEnumerable<string>> GetDownloaders()
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync($"{baseUrl}/Downloader/downloaders");
            var elements = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
            return elements ?? [];
        } 
        catch (Exception e)
        {
            logger.LogError("Failed to get downloaders with error: {error}", e.Message);
        }
        return [];
    }

    public async Task<DownloaderData?> GetDownloaderConfiguration(string downloader)
    {
        var client = httpClientFactory.CreateClient();
        
        try
        {
            var response = await client.GetAsync($"{baseUrl}/Downloader/{downloader}/configuration");
            var parser = await response.Content.ReadFromJsonAsync<DownloaderData>();
            return parser ?? null;
        }
        catch (Exception e)
        {
            logger.LogError("Failed to get downloader configuration with error: {error}", e.Message);
        }

        return null;
    }

    public async Task<bool> Reparse(string parser)
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            using HttpResponseMessage response = await client.PostAsync($"{baseUrl}/Downloader/{parser}/reparse", null);
            var returnStatusCode = response.StatusCode;
            return returnStatusCode == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            logger.LogError("Failed to reparse with error: {error}", e.Message);
            return false;
        }
    }
    
    public async Task<UploadResult> UploadFiles(string parser, MultipartFormDataContent content)
    {
        
        var client = httpClientFactory.CreateClient();
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/Downloader/{parser}/upload");
            request.Content = content;
            var response = await client.SendAsync(request);
            var res = response.Content;
            var responseString = await res.ReadAsStringAsync();
            if (responseString.Contains("The specified blob already exists."))
            {
                logger.LogWarning("Failed to upload file with error: {error}", Result.AlreadyExists);
                return new UploadResult(false, "The specified blob already exists.", Result.AlreadyExists);
            }
            
            if (responseString.Contains("invalid literal for int() with base 10: '\"E'\""))
            {
                logger.LogWarning("Failed to upload file with error: {error}", Result.FileFormatError);
                return new UploadResult(false, "Wrong file content.", Result.FileFormatError);
            }
            
            var returnStatusCode = response.StatusCode;
            if (returnStatusCode == HttpStatusCode.OK)
            {
                return new UploadResult( true, "", Result.UploadSuccess);
            }
            else
            {
                return new UploadResult( false, response.Content.ReadAsStringAsync().Result, Result.UploadError);
            }
        }
        catch (Exception e)
        {
            logger.LogError("Failed to upload file with error: {error}", e.Message);
            return new UploadResult(false, e.Message, Result.ExceptionOccured);
        }
    }
    
    public async Task<bool> ConfigureDownloader(string parser, string url, string backupUrl, string secretName, string pollingRate)
    {
        
        var client = httpClientFactory.CreateClient();
        
        try
        {
            using var jsonContent = SetupJsonContent(parser, url, backupUrl, secretName, pollingRate, [], []);
            using HttpResponseMessage response = await client.PutAsync($"{baseUrl}/Downloader/{parser}/configure", jsonContent);
            var returnStatusCode = response.StatusCode;
            return returnStatusCode == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            logger.LogError("Failed to configure downloader with error: {error}", e.Message);
        }
        return false;
    }

    public async Task<List<bool>> TestConnection(string parser, string url, string backupUrl, string secretName, string pollingRate)
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            using var jsonContent = SetupJsonContent(parser, url, backupUrl, secretName, pollingRate, [], []);
            Console.WriteLine(jsonContent.ReadAsStringAsync().Result);
            using HttpResponseMessage response  = await client.PostAsync($"{baseUrl}/test", jsonContent);
            
            var jsonResponse = await response.Content.ReadFromJsonAsync<List<bool>>();
            return jsonResponse ?? [];
        }
        catch (Exception e)
        {
            logger.LogError("Failed to test connection with error: {error}", e.Message);
        }
        return [];
    }
    
    private static StringContent SetupJsonContent(string parser, string url, string backupUrl, string secretName, string pollingRate, List<string> parameters, List<string> filters)
    {
        StringContent? jsonContent = null;
        try
        {
            jsonContent = new StringContent(
                JsonSerializer.Serialize(new DownloaderData()
                {
                    Name = parser,
                    DownloadUrl = url ?? "",
                    Parser = "",
                    BackUpUrl = backupUrl ?? "",
                    SecretName = secretName ?? "",
                    PollingRate = pollingRate ?? "",
                    Filters =  [],
                    Parameters = [],
                }),
                Encoding.UTF8,
                "application/json");
            return jsonContent;
        }
        catch
        {
            jsonContent?.Dispose();
            throw;
        }
    }

    public async Task<List<User>> GetUsers()
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync($"{baseUrl}/Auth/users");
            var users = await response.Content.ReadFromJsonAsync<List<User>>();
            return users ?? [];
        }
        catch (Exception e)
        {
            logger.LogError("Failed to get users with error: {error}", e.Message);
        }
        return [];
    }
    
    public async Task<List<Role>> GetUserRoles(string userId)
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync($"{baseUrl}/Auth/users/{userId}/roles");
            var roles = await response.Content.ReadFromJsonAsync<List<Role>>();
            return roles ?? [];
        }
        catch (Exception e)
        {
            logger.LogError("Failed to get user's roles with error: {error}", e.Message);
        }
        return [];
    }
    
    public async Task<bool> RemoveUserRole(string userId, RoleData roles)
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/Auth/users/{userId}/roles")
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
            logger.LogError("Failed to remove user's role with error: {error}", e.Message);
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
            using HttpResponseMessage response = await client.PostAsync($"{baseUrl}/Auth/users/{userId}/roles", jsonContent);
            var returnStatusCode = response.StatusCode;
                return returnStatusCode == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            logger.LogError("Failed to get update user's role with error: {error}", e.Message);
        }
        return false;
    }
    public async Task<List<Role>> GetRoles()
    {
        var client = httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync($"{baseUrl}/Auth/roles");
            var roles = await response.Content.ReadFromJsonAsync<List<Role>>();
            return roles ?? [];
        }
        catch (Exception e)
        {
            logger.LogError("Failed to get roles with error: {error}", e.Message);
        }
        return [];
    }
}