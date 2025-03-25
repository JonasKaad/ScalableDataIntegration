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
        var jobId = string.IsNullOrEmpty(data.ParserUrl) ? 
            _backgroundJobClient.Enqueue<DirectDownloadJob>(x => x.Download(data)) :
            _backgroundJobClient.Enqueue<BaseDownloaderJob>(x => x.Download(data));
        return jobId;
    }

    public string ScheduleOrUpdateRecurringDownload(DownloaderData data)
    {
        if (string.IsNullOrEmpty(data.ParserUrl))
        {
            _recurringJobManager.AddOrUpdate<DirectDownloadJob>(data.Name, x => x.Download(data), data.PollingRate);
        }
        else
        {
            _recurringJobManager.AddOrUpdate<BaseDownloaderJob>(data.Name, x => x.Download(data), data.PollingRate);
        }
        return data.Name;
    }
}