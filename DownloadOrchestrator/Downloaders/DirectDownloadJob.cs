using Azure.Storage.Blobs;
using CommonDis.Models;
using CommonDis.Services;
using DownloadOrchestrator.Services;

namespace DownloadOrchestrator.Downloaders;

public class DirectDownloadJob : BaseDownloaderJob
{
    private readonly ILogger<DirectDownloadJob> _logger;

    public DirectDownloadJob(ILogger<DirectDownloadJob> logger, StatisticsDatabaseService statisticsContext, SecretService secretService, ParserRegistry parserRegistry, FilterRegistry filterRegistry, CommonService commonService) 
        : base(logger, statisticsContext, secretService, parserRegistry, filterRegistry, commonService)
    {
        _logger = logger;
    }

    public new async Task Download(DownloaderData data)
    {
        try
        {
            var secret = string.IsNullOrEmpty(data.SecretName) ? data.Name : data.SecretName;
            var downloaderSecret = await SecretService.GetSecretAsync(secret);
            var tokenName = downloaderSecret?.TokenName ?? "";
            var token = downloaderSecret?.Token ?? "";
            var bytes = await FetchBytes(data.DownloadUrl, tokenName, token) 
                        ?? await FetchBytes(data.BackUpUrl, tokenName, token);
            if (bytes is null)
            {
                _logger.LogError("Unable to fetch bytes from {Url} or {BackUpUrl}", data.DownloadUrl, data.BackUpUrl);
                return;
            }
            await Log(data.Name, bytes, DateTime.UtcNow);
            var date = DateTime.UtcNow;
            _logger.LogInformation("Saving raw data to {Container} at {Date}-direct_raw.txt", data.Name, $"{date:yyyy/MM/dd/HHmm}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error downloading from {Url} or {BackUpUrl} using {Secret} ", data.DownloadUrl, data.BackUpUrl, data.SecretName);
        }
    }
}