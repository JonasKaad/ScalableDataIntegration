using DotNetEnv;
using DotNetEnv.Configuration;
using Downloader.Downloaders;
using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Services;
using DownloadOrchestrator.Utils;
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
        "https://www.airservicesaustralia.com/flextracks/text.asp?ver=1",
        "http://ausotparser.jonaskaad.com", 
        "AusotParser")
};
builder.Services.AddSingleton(downloaders)
    .AddDbContextFactory<StatisticsContext>(options =>
        options.UseNpgsql(connectionString))
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
}

app.UseHttpsRedirection();


app.MapControllers();

app.UseHangfireDashboard();

app.Run();

DownloaderData CreateDownloaderData(string url, string parserService, string name)
{
    return new DownloaderData{
        DownloadUrl = url,
        ParserUrl = parserService, 
        Name = name
    };
}