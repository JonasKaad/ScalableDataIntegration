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
            var response = await client.SendAsync(request);

            var status = response.StatusCode;
            if (status == HttpStatusCode.OK)
            {
                List<Role>? roles = JsonSerializer.Deserialize<List<Role>>(await response.Content.ReadAsStringAsync());
                return roles ?? [];
            }

            return BadRequest($"Failed to obtain roles: {status} \n {await response.Content.ReadAsStringAsync()}");
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
            var response = await client.SendAsync(request);
            var status = response.StatusCode;
            if (status == HttpStatusCode.OK)
            {
                
                List<Role>? userRoles = JsonSerializer.Deserialize<List<Role>>(await response.Content.ReadAsStringAsync());
                return userRoles ?? [];
            }

            return BadRequest($"Failed to obtain user's roles: {status} \n {await response.Content.ReadAsStringAsync()}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain user's roles");
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
    
    
    [Route("users/{userId}/roles")]
    [HttpPost]
    public async Task<ActionResult<bool>> UpdateUserRoles(string userId, [FromBody] RoleData roles)
    {
        try
        {
            var token = await _tokenCacheService.GetTokenAsync();

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://{AuthService.GetDomain()}/api/v2/users/{userId}/roles");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {token}");
            
            var payload = new {roles = roles.roles};
            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, null, "application/json");
            request.Content = content;
            
            var response = await client.SendAsync(request);
            
            if(response.StatusCode == HttpStatusCode.NoContent)
            {
                return Ok($"Updated user data.");
            }
            else return BadRequest("Failed to update user's roles. " + response.StatusCode + response.Content.ReadAsStringAsync());
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user's roles");
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
    
    [Route("users/{userId}/roles")]
    [HttpDelete]
    public async Task<ActionResult<bool>> DeleteUserRoles(string userId, [FromBody] RoleData roles)
    {
        try
        {
            var token = await _tokenCacheService.GetTokenAsync();

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://{AuthService.GetDomain()}/api/v2/users/{userId}/roles");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {token}");
            
            var payload = new {roles = roles.roles};
            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, null, "application/json");
            request.Content = content;
            
            var response = await client.SendAsync(request);
            
            if(response.StatusCode == HttpStatusCode.NoContent)
            {
                return Ok($"Updated & deleted user data.");
            }
            else return BadRequest("Failed to update user's roles. " + response.StatusCode + response.Content.ReadAsStringAsync());
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user's roles");
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
    
    
    [Route("users")]
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        try
        {
            var token = await _tokenCacheService.GetTokenAsync();


            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{AuthService.GetDomain()}/api/v2/users");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var response = await client.SendAsync(request);

            var status = response.StatusCode;
            if (status == HttpStatusCode.OK)
            {
                
                List<User>? users = JsonSerializer.Deserialize<List<User>>(await response.Content.ReadAsStringAsync());
                return users ?? [];
            }

            return BadRequest($"Failed to obtain users: {status} \n {await response.Content.ReadAsStringAsync()}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain users");
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}