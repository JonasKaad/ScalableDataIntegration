using CommonDis.Models;

namespace DownloadOrchestrator.Services;

public interface IDownloaderService
{
    public string ScheduleDownload(DownloaderData data);
    public string ScheduleOrUpdateRecurringDownload(DownloaderData data);
    public List<DownloaderData> GetRecurringJobs();
}