using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SkyPanel.Components.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonDis.Models.Auth0;

namespace SkyPanel.Components.Services;

public class ParserRoleSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ParserRoleSyncService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(24);

    public ParserRoleSyncService(
        IServiceScopeFactory scopeFactory,
        ILogger<ParserRoleSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var orchestratorClient = scope.ServiceProvider.GetRequiredService<OrchestratorClientService>();

                await SyncParserRoles(orchestratorClient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing parser roles");
            }

            await Task.Delay(_syncInterval, stoppingToken);
        }
    }

    public async Task SyncParserRoles(OrchestratorClientService orchestratorClient)
    {
        _logger.LogInformation("Starting parser role synchronization");
        
        var parsers = await orchestratorClient.GetDownloaders();
        
        if (parsers == null || !parsers.Any())
        {
            _logger.LogWarning("No parsers found to synchronize roles for");
            return;
        }

        var existingRoles = await orchestratorClient.GetRoles();
        var existingRoleNames = existingRoles.Select(r => r.name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        
        // For each parser, create a role if it doesn't exist
        foreach (var parser in parsers)
        {
            string roleName = $"{parser}";
            
            if (!existingRoleNames.Contains(roleName))
            {
                _logger.LogInformation("Creating missing role: {RoleName}", roleName);
                
                var newRole = new Role(Guid.NewGuid().ToString(), roleName, $"Access role for the {parser} parser");
                
                var success = await orchestratorClient.AddRole(newRole);
                if (!success)
                {
                    _logger.LogWarning("Failed to create role: {RoleName}", roleName);
                }
            }
        }
        
        _logger.LogInformation("Parser role synchronization completed");
    }
}