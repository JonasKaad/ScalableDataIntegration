using DownloadOrchestrator.Models;

namespace DownloadOrchestrator.Services;

public class AuthService
{
    private AuthModel _authModel;
    
    public AuthService(string domain, string clientId, string clientSecret)
    {
        _authModel = new AuthModel(domain, clientId, clientSecret);
    }

    public string GetDomain()
    {
        return _authModel.Domain;
    }
    public string GetClientId()
    {
        return _authModel.ClientId;
    }
    public string GetClientSecret()
    {
        return _authModel.ClientSecret;
    }
}