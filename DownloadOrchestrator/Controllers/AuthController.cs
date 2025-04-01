using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using DownloadOrchestrator.Models.Auth0;
using DownloadOrchestrator.Services;
using Microsoft.AspNetCore.Mvc;
namespace DownloadOrchestrator.Controllers;

public class AuthController : ControllerBase
{
    private AuthService AuthService  { get; set; }
    private readonly ILogger<AuthController> _logger;
    private readonly TokenCacheService _tokenCacheService;
    
    public AuthController(AuthService authService, ILogger<AuthController> logger, TokenCacheService tokenCacheService)
    {
        AuthService = authService;
        _logger = logger;
        _tokenCacheService = tokenCacheService;
    }

    [Route("roles")]
    [HttpGet]
    public async Task<ActionResult<List<Role>>> GetRoles()
    {
        try
        {
            var token = await _tokenCacheService.GetTokenAsync();


            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{AuthService.GetDomain()}/api/v2/roles");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var response = client.SendAsync(request);

            var status = response.Result.StatusCode;
            if (status == HttpStatusCode.OK)
            {
                List<Role>? roles = JsonSerializer.Deserialize<List<Role>>(response.Result.Content.ReadAsStringAsync().Result);
                return roles ?? [];
            }

            return BadRequest($"Failed to obtain roles: {status} \n {response.Result.Content.ReadAsStringAsync()}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain roles");
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
    
    
    [Route("users/{userId}/roles")]
    [HttpGet]
    public async Task<ActionResult<List<Role>>> GetUserRoles(string userId)
    {
        try
        {
            var token = await _tokenCacheService.GetTokenAsync();

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{AuthService.GetDomain()}/api/v2/users/{userId}/roles");

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var response = client.SendAsync(request);
            Console.WriteLine(response.Result);
            var status = response.Result.StatusCode;
            if (status == HttpStatusCode.OK)
            {
                
                List<Role>? userRoles = JsonSerializer.Deserialize<List<Role>>(response.Result.Content.ReadAsStringAsync().Result);
                return userRoles ?? [];
            }

            return BadRequest($"Failed to obtain user's roles: {status} \n {response.Result.Content.ReadAsStringAsync()}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain user's roles");
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
    
    
}