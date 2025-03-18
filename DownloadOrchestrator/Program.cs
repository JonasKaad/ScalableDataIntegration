using DotNetEnv;
using DotNetEnv.Configuration;
using Downloader.Downloaders;
using DownloadOrchestrator.Downloaders;
using DownloadOrchestrator.Models;
using DownloadOrchestrator.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());

var connectionString = "Server=" + Environment.GetEnvironmentVariable("SERVER") + ";" + 
                       "Database=" + Environment.GetEnvironmentVariable("DATABASE")+ ";"  + 
                       "Username=" + Environment.GetEnvironmentVariable("USER")+ ";"  + 
                       "Password=" + Environment.GetEnvironmentVariable("PASSWORD") + ";" ;

var dbContextFactory = new PooledDbContextFactory<StatisticsContext>(
    new DbContextOptionsBuilder<StatisticsContext>()
        .UseNpgsql(connectionString)
        .Options);
var downloaders = new List<IDownloader>
{
    CreateDownloader(
        "https://www.airservicesaustralia.com/flextracks/text.asp?ver=1",
        "http://ausotparser.jonaskaad.com", 
        "AusotParser",
        dbContextFactory)
};
builder.Services.AddSingleton(downloaders);
builder.Services.AddSingleton<PooledDbContextFactory<StatisticsContext>>(dbContextFactory);
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapControllers();

app.Run();

IDownloader CreateDownloader(string url, string parserService, string name, PooledDbContextFactory<StatisticsContext> contextFactory)
{
    var client = new DisDownloaderClient(url);
    return new BaseDownloader(client, parserService, name, contextFactory);
}