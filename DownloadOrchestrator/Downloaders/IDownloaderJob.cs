using CommonDis.Models;

namespace DownloadOrchestrator.Downloaders;

public interface IDownloaderJob
{
    public Task Download(DownloaderData data);
}