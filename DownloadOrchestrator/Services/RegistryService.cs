namespace DownloadOrchestrator.Services;

public abstract class RegistryService
{
    private Dictionary<string, string> _services = new Dictionary<string, string>();
    private Dictionary<string, DateTime> _serviceDateTimes = new Dictionary<string, DateTime>();
    private CancellationTokenSource _cts = new CancellationTokenSource();


    public RegistryService()
    {
        _ = CheckHeartbeats();
    }
    
    public void RefreshService(string serviceName)
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
    
    public void RegisterService(string serviceName, string url)
    {
        if(_services.TryGetValue(serviceName, out var service))
        {
            if (service != url)
            {
                throw new Exception($"{serviceName} exists with a different URL");
            }
            throw new ArgumentException($"{serviceName} already exists");
        }

        try
        {
            var uri = new Uri(url);
        }
        catch (UriFormatException)
        {
            throw new UriFormatException("Invalid URL");
        }
        _serviceDateTimes.Add(serviceName, DateTime.Now);
        _services.Add(serviceName, url);
    }
    
    public void DeRegisterService(string serviceName)
    {
        if (_services.ContainsKey(serviceName))
        {
            _services.Remove(serviceName);
        }
        else
        {
            throw new KeyNotFoundException($"{serviceName} not found");
        }
    }

    private async Task CheckHeartbeats()
    {
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(1), _cts.Token);
                
                var services = _serviceDateTimes.Keys.ToList();
                foreach (var service in services)
                {
                    if (_serviceDateTimes.TryGetValue(service, out var datetime) && datetime.AddHours(1) < DateTime.Now)
                    {
                        DeRegisterService(service);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Heartbeat check cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in heartbeat check: {ex.Message}");
        }
    }
    
    public string GetService(string serviceName)
    {
        var service = _services.FirstOrDefault(a => a.Key.Equals(serviceName)).Value;
        if (service != null)
        {
            return service;
        }
        throw new KeyNotFoundException($"{serviceName} not found");
    }

    public List<string> GetServices()
    {
        return _services.Keys.ToList();
    }
    
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}