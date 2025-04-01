using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace SkyPanel.Components;
using SkyPanel.Components.Services;

public partial class Dashboard : ComponentBase
{
    [Inject] private ParserStateService ParserState { get; set; } = default!;
    [Inject] private OrchestratorClientService OrchestratorClient { get; set; } = default!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    
    private bool _canAccessParsers = false;
    protected override async Task OnInitializedAsync()
    {
        await CheckParserAccess();
    }
    
    private async Task CheckParserAccess()
    {
        var authState = await AuthenticationStateTask;
        var user = authState.User;
        
        // Admin always has access
        if (user.IsInRole("Admin"))
        {
            _canAccessParsers = true;
            return;
        }
        
        // Check if user has access to any parser
        var availableParsers = await OrchestratorClient.GetDownloaders();
        _canAccessParsers = availableParsers.Any(parser => user.IsInRole(parser));
    }
}