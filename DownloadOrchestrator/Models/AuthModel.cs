namespace DownloadOrchestrator.Models;

public class AuthModel(string domain, string clientId, string clientSecret)
{
    public string Domain { get; } = domain;
    public string ClientId { get; } = clientId;
    public string ClientSecret { get; } = clientSecret;
}