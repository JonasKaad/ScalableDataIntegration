using CommonDis.Services;
using DotNetEnv;
using DotNetEnv.Configuration;
using MetaDataParser.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());

// Add services to the container.
builder.Services
    .AddSingleton<CommonService>()
    .AddHostedService<HeartbeatService>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<HeartbeatService>>();
        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL");
        var name = Environment.GetEnvironmentVariable("PARSER_NAME");
        var parserUrl = Environment.GetEnvironmentVariable("PARSER_URL");
        var interval = TimeSpan.FromSeconds(10);
        return new HeartbeatService(logger, baseUrl, name, parserUrl, interval);
    })
    .AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<MetaDataService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();