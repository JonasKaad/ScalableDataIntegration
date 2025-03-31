namespace DownloadOrchestrator.Services;

public class TokenCacheService
{
    private string _cachedToken = string.Empty;
    private DateTime _expirationTime = DateTime.MinValue;
    private readonly AuthService AuthService;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger<TokenCacheService> _logger;
    private readonly HttpClient _httpClient;

    public TokenCacheService(AuthService authService, ILogger<TokenCacheService> logger)
    {
        AuthService = authService;
        _logger = logger;
        _httpClient = new HttpClient();
    }
    public async Task<string> GetTokenAsync()
        {
            // Check if token is still valid (with some buffer time of 2 hours)
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow.AddMinutes(120) < _expirationTime)
            {
                return _cachedToken;
            }

            // Token needs refreshing
            await _semaphore.WaitAsync();
            try
            {
                // Double-check after acquiring the lock (to prevent multiple refreshes)
                if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow.AddMinutes(120) < _expirationTime)
                {
                    return _cachedToken;
                }
                await RefreshTokenAsync();
                return _cachedToken;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task RefreshTokenAsync()
        {
            _logger.LogInformation("Refreshing Auth0 API token");
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://{AuthService.GetDomain()}/oauth/token");
            
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", $"{AuthService.GetClientId()}" },
                { "client_secret", $"{AuthService.GetClientSecret()}" },
                { "audience", $"https://{AuthService.GetDomain()}/api/v2/" }
            };
            
            request.Content = new FormUrlEncodedContent(parameters);
            
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(responseContent);
                
                _cachedToken = tokenResponse.access_token;
                // Set expiration time based on the token's expires_in value (in seconds)
                _expirationTime = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);
                
                _logger.LogInformation("Auth0 API token refreshed successfully. Expires at: {ExpiryTime}", _expirationTime);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to refresh Auth0 API token: {StatusCode} - {ErrorContent}", 
                    response.StatusCode, errorContent);
                
                // Keep the old token if refresh fails
                if (string.IsNullOrEmpty(_cachedToken))
                {
                    throw new Exception("Failed to obtain Auth0 API token");
                }
            }
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