using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Models;
using Hangfire;

namespace DownloadOrchestrator.Services;

public class DownloaderService : IDownloaderService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public DownloaderService(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }
    public string ScheduleDownload(DownloaderData data)
    {
        
        var jobId = _backgroundJobClient.Enqueue<IDownloaderJob>(x => x.Download(data));
        return jobId;
    }

    public string ScheduleOrUpdateRecurringDownload(DownloaderData data)
    {
        _recurringJobManager.AddOrUpdate<IDownloaderJob>(data.Name, x => x.Download(data), data.PollingRate);
        return data.Name;
    }
}