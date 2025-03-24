using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using SkyPanel.Components;
using SkyPanel.Components.Services;
using DotNetEnv;
using DotNetEnv.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Load env variables
builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());
builder.Services.AddMudServices();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ParserStateService>();
builder.Services.AddScoped<SecretCredentialsService>();
builder.Services.AddScoped<BlobManagerService>(provider =>
{
    var connectionString = Env.GetString("BLOB_CONNECTION_STRING");
    return new BlobManagerService(connectionString);
});
//Database service setup
builder.Services.AddDbContext<StatisticsDatabaseService>(options =>
{
    // Npgsql formatted connection string
    var connectionString = "Server=" + Env.GetString("SERVER") + ";" + 
                           "Database=" + Env.GetString("DATABASE")+ ";"  + 
                           "Username=" + Env.GetString("USER")+ ";"  + 
                           "Password=" + Env.GetString("PASSWORD") + ";" ;
    options.UseNpgsql(connectionString);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();