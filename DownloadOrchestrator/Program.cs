using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DotNetEnv;
using DotNetEnv.Configuration;
using Downloader.Downloaders;
using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Services;
using Microsoft.EntityFrameworkCore;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());

var connectionString = "Server=" + Environment.GetEnvironmentVariable("SERVER") + ";" + 
                       "Database=" + Environment.GetEnvironmentVariable("DATABASE")+ ";"  + 
                       "Username=" + Environment.GetEnvironmentVariable("USER")+ ";"  + 
                       "Password=" + Environment.GetEnvironmentVariable("PASSWORD") + ";" ;
var downloaders = new List<DownloaderData>
{
    CreateDownloaderData(
        "https://www.airservicesausralia.com/flextracks/text.asp?ver=1",
        "https://www.airservicesaustralia.com/flextracks/text.asp?ver=1",
        "http://ausotparser.jonaskaad.com", 
        "AusotParser",
        "*/10 * * * *")
};

builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(downloaders)
    .AddDbContextFactory<StatisticsContext>(options =>
        options.UseNpgsql(connectionString))
    .AddScoped<SecretService>(s =>
    {
        var userAssignedClientId = new ResourceIdentifier(Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"));

        var credential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = userAssignedClientId
            });
        var keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
        var kvUri = "https://" + keyVaultName + ".vault.azure.net";

        var client = new SecretClient(new Uri(kvUri), credential);
        return new SecretService(client);
    })
    .AddScoped<IDownloaderJob, BaseDownloaderJob>()
    .AddScoped<IDownloaderService, DownloaderService>()
    .AddHangfire(config => config.UseInMemoryStorage())
    .AddHangfireServer()
    .AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseHangfireDashboard();

app.Run();

DownloaderData CreateDownloaderData(string url, string backup, string parserService, string name, string pollingRate)
{
    return new DownloaderData{
        DownloadUrl = url,
        BackUpUrl = backup,
        ParserUrl = parserService, 
        Name = name,
        PollingRate = pollingRate
    };
}