using Microsoft.AspNetCore.Components;
using SkyPanel.Components.Models;

namespace SkyPanel.Components.Services;

public class SecretCredentialsService
{
    private IDictionary<string, IDictionary<string, string>> _secretDictionary;
    
    public SecretCredentialsService()
    {
        _secretDictionary = GetAwsSecrets();
    }
    public string GetUsername(string parserName)
    {
        if (string.IsNullOrEmpty(parserName))
        {
            return string.Empty;
        }
        
        if (_secretDictionary.TryGetValue(parserName, out var credentials))
        {
            return credentials.TryGetValue("username", out var credential) ? credential : string.Empty;
        }
        return string.Empty;
    }
    
    public string GetPassword(string parserName)
    {
        if (string.IsNullOrEmpty(parserName))
        {
            return string.Empty;
        }
        
        if (_secretDictionary.TryGetValue(parserName, out var credentials))
        {
            return credentials.TryGetValue("password", out var credential) ? credential : string.Empty;
        }
        return string.Empty;
    }
    
    public IEnumerable<string> GetParserSecretNames()
    {
        return GetAwsSecrets().Keys;
    }

    // This method is used to simulate the retrieval of secrets from AWS Secrets Manager
    private Dictionary<string, IDictionary<string, string>> GetAwsSecrets()
    {
        return new Dictionary<string, IDictionary<string, string>>
        {
            { "WeatherParser", new Dictionary<string, string> { { "username", "username1" }, { "password", "password1" } } },
            { "MetarParser", new Dictionary<string, string> { { "username", "username2" }, { "password", "password2" } } },
            { "SigmetParser", new Dictionary<string, string> { { "username", "username3" }, { "password", "password3" } } },
            { "BufrrrrParser", new Dictionary<string, string> { { "username", "jonas" }, { "password", "1233334" } } },
            { "NatTrackParser", new Dictionary<string, string> { { "username", "victor" }, { "password", "4321" } } },
            { "AirepParser", new Dictionary<string, string> { { "username", "username4" }, { "password", "password4" } } },
            { "NotamParser", new Dictionary<string, string> { { "username", "username5" }, { "password", "password5" } } },
            { "TafParser", new Dictionary<string, string> { { "username", "username6" }, { "password", "password6" } } },
            { "GfsParser", new Dictionary<string, string> { { "username", "username7" }, { "password", "password7" } } },
            { "PacotParser", new Dictionary<string, string> { { "username", "username8" }, { "password", "password8" } } },
            { "TrackParser", new Dictionary<string, string> { { "username", "username9" }, { "password", "password9" } } },
            { "FlightPlanParser", new Dictionary<string, string> { { "username", "username10" }, { "password", "password10" } } },
            { "RadarParser", new Dictionary<string, string> { { "username", "username11" }, { "password", "password11" } } },
            { "SatelliteParser", new Dictionary<string, string> { { "username", "username12" }, { "password", "password12" } } },
            { "OceanicRouteParser", new Dictionary<string, string> { { "username", "username13" }, { "password", "password13" } } },
            { "WindParser", new Dictionary<string, string> { { "username", "username14" }, { "password", "password14" } } },
            { "TemperatureParser", new Dictionary<string, string> { { "username", "username15" }, { "password", "password15" } } },
            { "TurbulenceParser", new Dictionary<string, string> { { "username", "username16" }, { "password", "password16" } } },
            { "IcingParser", new Dictionary<string, string> { { "username", "username17" }, { "password", "password17" } } },
            { "CloudParser", new Dictionary<string, string> { { "username", "username18" }, { "password", "password18" } } },
            { "StormParser", new Dictionary<string, string> { { "username", "username19" }, { "password", "password19" } } },
            { "AviationWeatherParser", new Dictionary<string, string> { { "username", "username20" }, { "password", "password20" } } },
            { "AirspaceParser", new Dictionary<string, string> { { "username", "username21" }, { "password", "password21" } } },
            { "TrafficParser", new Dictionary<string, string> { { "username", "username22" }, { "password", "password22" } } },
            { "RunwayParser", new Dictionary<string, string> { { "username", "username23" }, { "password", "password23" } } },
            { "AirportParser", new Dictionary<string, string> { { "username", "username24" }, { "password", "password24" } } },
            { "FuelDataParser", new Dictionary<string, string> { { "username", "username25" }, { "password", "password25" } } },
            { "ClearanceParser", new Dictionary<string, string> { { "username", "username26" }, { "password", "password26" } } },
            { "LoadSheetParser", new Dictionary<string, string> { { "username", "username27" }, { "password", "password27" } } },
            { "PirepParser", new Dictionary<string, string> { { "username", "username28" }, { "password", "password28" } } },
            { "FIRParser", new Dictionary<string, string> { { "username", "username29" }, { "password", "password29" } } },
            { "SectorParser", new Dictionary<string, string> { { "username", "username30" }, { "password", "password30" } } }
        };
    
    }
}