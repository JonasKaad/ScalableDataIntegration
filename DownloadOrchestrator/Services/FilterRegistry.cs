using CommonDis.Models;

namespace DownloadOrchestrator.Services;

public class FilterRegistry : RegistryService
{
    private Dictionary<string, FilterData> _services = new Dictionary<string, FilterData>();
    private ILogger<FilterRegistry> _logger;
    
    public FilterRegistry(ILogger<FilterRegistry> logger)
    {
        _logger = logger;
    }

    public string GetFilterUrl(string filterName)
    {
        return _services[filterName].Url;
    }
    
    public FilterDto? GetService(string serviceName)
    {
        var service = _services.FirstOrDefault(a => a.Key.Equals(serviceName, StringComparison.InvariantCultureIgnoreCase)).Value;
        var filter = new FilterDto
        {
            Name = service?.Name,
            Parameters = service?.Parameters,
        };
        return filter;
    }
    
    public List<string> GetServices()
    {
        return _services.Keys.ToList();
    }
    
    public void RegisterService(string serviceName, FilterData settings)
    {
        if(_services.TryGetValue(serviceName, out var service))
        {
            if (!EqualFilters(service, settings))
            {
                _logger.LogInformation("Registering {Service}, though different settings were detected", serviceName);
            }

            return;
        }
        
        _serviceDateTimes.Add(serviceName, DateTime.Now);
        _services.Add(serviceName, settings);
    }

    private bool EqualFilters(FilterData registered, FilterData newFilter)
    {
        var same = true;
        same &= registered.Name == newFilter.Name;
        same &= registered.Url == newFilter.Url;
        same &= registered.Parameters.Count == newFilter.Parameters.Count;
        if (!same) return same;
        for (var i = 0; i < registered.Parameters.Count; i++)
        {
            var regParam = registered.Parameters.ElementAt(i);
            var newParam = newFilter.Parameters.ElementAt(i);
            same &= regParam.Key == newParam.Key;
            same &= regParam.Value == newParam.Value;
        }

        return same;
    }

    public new void DeRegisterService(string serviceName)
    {
        if (_services.ContainsKey(serviceName))
        {
            _serviceDateTimes.Remove(serviceName);
            _services.Remove(serviceName);
        }
        else
        {
            _logger.LogInformation("{Service} not found", serviceName);
        }
    }
    
    public new void RefreshService(string serviceName)
    {
        if (_services.ContainsKey(serviceName))
        {
            _serviceDateTimes[serviceName] = DateTime.Now;
        }
        else
        {
            throw new KeyNotFoundException($"{serviceName} not found");
        }
    }
}