using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DotNetEnv;
using DotNetEnv.Configuration;
using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Services;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.PostgreSql;
using Serilog;
using Serilog.Sinks.Datadog.Logs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
}
else
{
    var datadogConfiguration = new DatadogConfiguration() { Url = "https://http-intake.logs.us5.datadoghq.com", UseSSL = true, UseTCP = false};
    Log.Logger = new LoggerConfiguration()
        .WriteTo.DatadogLogs(
            apiKey: Environment.GetEnvironmentVariable("DD_API_KEY"),
            configuration: datadogConfiguration, 
            service: "DownloadOrchestrator"
            )
        .CreateLogger();
}

var connectionString = "Server=" + Environment.GetEnvironmentVariable("SERVER") + ";" + 
                       "Database=" + Environment.GetEnvironmentVariable("DATABASE")+ ";"  + 
                       "Username=" + Environment.GetEnvironmentVariable("USER")+ ";"  + 
                       "Password=" + Environment.GetEnvironmentVariable("PASSWORD") + ";" ;
builder.Services.AddSwaggerGen();
builder.Services
    .AddSerilog()
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
        var logger = s.GetService<Logger<SecretService>>();
        return new SecretService(client, logger!);
    })
    .AddScoped<IDownloaderJob, BaseDownloaderJob>()
    .AddScoped<IDownloaderService, DownloaderService>()
    .AddHangfire(config => config.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)))
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
app.UseSerilogRequestLogging();

try
{
    Log.Information("Starting app");
    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "DownloadOrchestrator terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}