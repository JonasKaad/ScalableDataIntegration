using CommonDis.Models;
using CommonDis.Services;

namespace SkyPanel.Components.Services;

public class SecretCredentialsService
{
    private readonly SecretService _secretService;
    
    public SecretCredentialsService(SecretService secretService)
    {
        _secretService = secretService;
    }
    
    public async Task<DisSecret> GetSecret(string parserName)
    {
        if (string.IsNullOrEmpty(parserName))
        {
            return new DisSecret();
        }
        
        var credentials = await _secretService.GetSecretAsync(parserName);
        
        return credentials ?? new DisSecret();
    }
    
    public async Task<List<string>> GetParserSecretNames()
    {
        return await _secretService.GetSecretNamesAsync();
    }
    
    public async Task<bool> HasSecret(string parserName)
    {
        return await _secretService.GetSecretAsync(parserName) != null;
    }
}