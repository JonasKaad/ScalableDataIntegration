using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CommonDis.Services;
using DotNetEnv;
using DotNetEnv.Configuration;
using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Datadog.Logs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    Log.Logger = new LoggerConfiguration().WriteTo.Console()
        .CreateLogger();
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
        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
        .CreateLogger();
}

var connectionString = "Server=" + Environment.GetEnvironmentVariable("SERVER") + ";" + 
                       "Database=" + Environment.GetEnvironmentVariable("DATABASE")+ ";"  + 
                       "Username=" + Environment.GetEnvironmentVariable("USER")+ ";"  + 
                       "Password=" + Environment.GetEnvironmentVariable("PASSWORD") + ";" ;
builder.Services.AddSingleton<AuthService>(_ =>
{
    var domain = Env.GetString("AUTH0_DOMAIN");
    var clientId = Env.GetString("AUTH0_CLIENT_ID");
    var clientSecret = Env.GetString("AUTH0_CLIENT_SECRET");
    return new AuthService(domain, clientId, clientSecret);
});
builder.Services.AddSingleton<TokenCacheService>();

builder.Services.AddSwaggerGen();
builder.Services
    .AddSerilog()
    .AddSingleton<CommonService>()
    .AddSingleton<ParserRegistry>()
    .AddSingleton<FilterRegistry>()
    .AddDbContextFactory<StatisticsDatabaseService>(options =>
        options.UseNpgsql(connectionString))
    .AddScoped<SecretService>(s =>
    {
        var azureClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        if (string.IsNullOrEmpty(azureClientId))
        {
            throw new InvalidOperationException("AZURE_CLIENT_ID environment variable is not set.");
        }
        
        var keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
        var kvUri = "https://" + keyVaultName + ".vault.azure.net";
        TokenCredential credential;
        var accessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN"); 
        
        if (!string.IsNullOrEmpty(accessToken))
        {
            credential = new StaticTokenCredential(accessToken);
        }
        else
        {
            var userAssignedClientId = new ResourceIdentifier(azureClientId);
            credential = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = userAssignedClientId
                });
        }
        var client = new SecretClient(new Uri(kvUri), credential);
        var logger = s.GetService<Logger<SecretService>>();
        return new SecretService(client, logger!);
    })
    .AddScoped<IDownloaderJob, BaseDownloaderJob>()
    .AddScoped<IDownloaderService, DownloaderService>()
    .AddHangfire(config =>
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            config.UseInMemoryStorage();
        }
        else
        {
            config.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString));
        }
    })
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

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.UseHangfireDashboard();

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



public class StaticTokenCredential : TokenCredential
{
    private readonly string _token;

    public StaticTokenCredential(string token)
    {
        _token = token;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new AccessToken(_token, DateTimeOffset.Now.AddHours(1));
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new ValueTask<AccessToken>(GetToken(requestContext, cancellationToken));
    }
}