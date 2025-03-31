using AusotParser.Services;
using DotNetEnv;
using DotNetEnv.Configuration;
using Serilog;
using Serilog.Sinks.Datadog.Logs;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());

var config = new DatadogConfiguration() { Url = "https://http-intake.logs.us5.datadoghq.com", UseSSL = true, UseTCP = false};
Log.Logger = new LoggerConfiguration()
    .WriteTo.DatadogLogs(
        apiKey: Environment.GetEnvironmentVariable("DD_API_KEY"),
        configuration: config, 
        service: "AusotParser"
    )
    .CreateLogger();

// Add services to the container.
builder.Services
    .AddSerilog()
    .AddHttpClient()
    .AddHostedService<HeartbeatService>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<HeartbeatService>>();
        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
        var baseUrl = "http://localhost:5162";
        var parserName = "ausotparser";
        var parserUrl = "http://ausotparser.jonaskaad.com";
        var interval = TimeSpan.FromMinutes(30);
        return new HeartbeatService(logger, baseUrl, parserName, parserUrl, interval);
    })
    .AddGrpc();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AusotParserService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

var registered = false;
while (!registered)
{
    var response = await HeartbeatService.RegisterParser(new HttpClient(), "http://localhost:5162/ausotparser/register", "http://ausotparser.jonaskaad.com", new Logger<HeartbeatService>(new LoggerFactory()));
    if(response)
    {
        registered = true;
    }
    else
    {
        Console.WriteLine("Failed to register parser. Retrying in 1 second");
        await Task.Delay(1000);
    }
}
try
{
    Log.Information("Starting AusotParser...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
