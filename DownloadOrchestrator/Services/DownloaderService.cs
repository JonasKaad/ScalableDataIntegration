using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Models;
using Hangfire;
using Hangfire.Storage;

namespace DownloadOrchestrator.Services;

public class DownloaderService : IDownloaderService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly JobStorage _jobStorage;

    public DownloaderService(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, JobStorage jobStorage)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _jobStorage = jobStorage;
    }
    public string ScheduleDownload(DownloaderData data)
    {
        var jobId = string.IsNullOrEmpty(data.Parser) ? 
            _backgroundJobClient.Enqueue<DirectDownloadJob>(x => x.Download(data)) :
            _backgroundJobClient.Enqueue<BaseDownloaderJob>(x => x.Download(data));
        return jobId;
    }

    public string ScheduleOrUpdateRecurringDownload(DownloaderData data)
    {
        if (string.IsNullOrEmpty(data.Parser))
        {
            _recurringJobManager.AddOrUpdate<DirectDownloadJob>(data.Name, x => x.Download(data), data.PollingRate);
        }
        else
        {
            _recurringJobManager.AddOrUpdate<BaseDownloaderJob>(data.Name, x => x.Download(data), data.PollingRate);
        }
        return data.Name;
    }

    public List<DownloaderData> GetRecurringJobs()
    {
        var jobs = _jobStorage.GetReadOnlyConnection().GetRecurringJobs();
        return jobs
            .Where(job => job.Job.Args.Count > 0 && job.Job.Args[0] is DownloaderData)
            .Select(job => job.Job.Args[0] as DownloaderData)
            .Where(data => data != null)
            .ToList();
    }
}