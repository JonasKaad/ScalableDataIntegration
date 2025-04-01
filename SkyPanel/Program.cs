using Auth0.AspNetCore.Authentication;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using SkyPanel.Components;
using SkyPanel.Components.Services;
using DotNetEnv;
using DotNetEnv.Configuration;
using MudBlazor;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Load env variables
builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());
builder.Services.AddMudServices(config =>
    {
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    })
    .AddHttpClient()
    .AddScoped<SecretService>(s =>
    {
        var userAssignedClientId = new ResourceIdentifier(Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? "abc");

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
    .AddScoped<ParserStateService>()
    .AddScoped<SecretCredentialsService>()
    .AddScoped<OrchestratorClientService>(provider =>
    {
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var baseUrl = Env.GetString("DOWNLOAD_ORCHESTRATOR_URL");
        return new OrchestratorClientService(httpClientFactory, baseUrl);
    })
    .AddScoped<BlobManagerService>(_ =>
    {
        var connectionString = Env.GetString("BLOB_CONNECTION_STRING");
        return new BlobManagerService(connectionString);
    })
    .AddDbContext<StatisticsDatabaseService>(options =>
    {
        // Npgsql formatted connection string
        var connectionString = "Server=" + Env.GetString("SERVER") + ";" + 
                               "Database=" + Env.GetString("DATABASE")+ ";"  + 
                               "Username=" + Env.GetString("USER")+ ";"  + 
                               "Password=" + Env.GetString("PASSWORD") + ";" ;
        options.UseNpgsql(connectionString);
    })
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = Env.GetString("AUTH0_DOMAIN");
        options.ClientId =Env.GetString("AUTH0_CLIENT_ID");
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
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapStaticAssets();

app.MapGet("/Account/Login", async (HttpContext httpContext, string returnUrl = "/") =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
        .WithRedirectUri(returnUrl)
        .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/Account/Logout", async (HttpContext httpContext) =>
{
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
        .WithRedirectUri("/")
        .Build();

    await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();