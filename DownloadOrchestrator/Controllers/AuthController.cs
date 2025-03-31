using System.Net;
using System.Net.Http.Headers;
using DownloadOrchestrator.Services;
using Microsoft.AspNetCore.Mvc;
namespace DownloadOrchestrator.Controllers;

public class AuthController : ControllerBase
{
    private AuthService AuthService  { get; set; }
    
    public AuthController(AuthService authService)
    {
        AuthService = authService;
    }
    
    [Microsoft.AspNetCore.Mvc.Route("token")]
    [HttpGet]
    public ActionResult<string> GetKeys()
    {
        var client = new HttpClient();
        Console.WriteLine(AuthService.GetDomain());
        var request = new HttpRequestMessage(HttpMethod.Post, $"https://{AuthService.GetDomain()}/oauth/token");
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" }, 
            { "client_id", $"{AuthService.GetClientId()}" },
            { "client_secret", $"{AuthService.GetClientSecret()}" },
            { "audience", $"https://{AuthService.GetDomain()}/api/v2/" }
        };
        var encodedContent = new FormUrlEncodedContent(parameters);
        request.Content = encodedContent;
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        
        var response = client.Send(request);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(responseContent);
        
            // Parse the JSON to extract the access token
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(responseContent);
            Console.WriteLine("Token: " + tokenResponse.access_token);
            return tokenResponse.access_token;
            
        }
        return BadRequest("Failed to obtain access token");
    }
    
    // Class to deserialize the token response
    private class TokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
    }
}