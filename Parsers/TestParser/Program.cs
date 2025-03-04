using Microsoft.AspNetCore.Server.Kestrel.Core;
using TestParser.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddHealthChecks();

builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenAnyIP(8084, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
    o.ListenAnyIP(8081, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<TestParserService>();
app.MapHealthChecks("/health").RequireHost("*:8084");
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();