using System.Text.Json;
using Azure.Security.KeyVault.Secrets;
using CommonDis.Models;
using Microsoft.Extensions.Logging;

namespace CommonDis.Services;

public class SecretService
{
    private DisSecretModel _secrets;
    private string _currentVersion;
    private readonly SecretClient _client;
    private DateTime _lastUpdated;
    private readonly ILogger<SecretService> _logger;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(5);

    public SecretService(SecretClient client, ILogger<SecretService> logger)
    {
        _client = client;
        _lastUpdated = DateTime.UtcNow.AddYears(-420);
        _logger = logger;
    }

    private async Task LoadSecretsAsync()
    {
        var rawSecret = await _client.GetSecretAsync("DataIntegrationService");
        if (string.IsNullOrEmpty(rawSecret.Value.Value))
        {
            _logger.LogWarning("Tried to fetch secret 'DataIntegrationService' but it does not exist");
            throw new ArgumentException("The specified secret was not found");
        }
        _currentVersion = rawSecret.Value.Properties.Version;
        _secrets = JsonSerializer.Deserialize<DisSecretModel>(rawSecret.Value.Value)!;
        _lastUpdated = DateTime.UtcNow;
    }

    public async Task<DisSecret?> GetSecretAsync(string secretName)
    {
        if (DateTime.UtcNow - _lastUpdated > _refreshInterval)
        {
            await CheckSecrets();
        }
        var found = _secrets.Secrets.TryGetValue(secretName, out var secret);
        return found ? secret : null;
    }

    public DisSecret? GetSecret(string secretName)
    {
        if (DateTime.UtcNow - _lastUpdated > _refreshInterval)
        {
            CheckSecrets().Wait();
        }
        var found = _secrets.Secrets.TryGetValue(secretName, out var secret);
        return found ? secret : null;
    }

    private async Task CheckSecrets()
    {
        var secret = await _client.GetSecretAsync("DataIntegrationService");
        var version = secret.Value.Properties.Version;
        if (version != _currentVersion)
        {
            await LoadSecretsAsync();
        };
    }

    public async Task AddSecretAsync(string secretName, DisSecret secret)
    {
        _secrets.Secrets[secretName] = secret;
        var serialized = JsonSerializer.Serialize(_secrets);
        await _client.SetSecretAsync("DataIntegrationService", serialized);
    }
    
    public void AddSecret(string secretName, DisSecret secret)
    {
        _secrets.Secrets[secretName] = secret;
        var serialized = JsonSerializer.Serialize(_secrets);
        _client.SetSecret("DataIntegrationService", serialized);
    }

    public async Task<List<string>> GetSecretNamesAsync()
    {
        if (DateTime.UtcNow - _lastUpdated > _refreshInterval)
        {
            await CheckSecrets();
        }
        return _secrets.Secrets.Keys.ToList();
    }

}