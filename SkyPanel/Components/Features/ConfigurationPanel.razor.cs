using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Dialogs;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Features;

public partial class ConfigurationPanel : ComponentBase
{
    [Inject] private ParserStateService ParserState { get; set; } = default!;
    
    [Inject] private SecretCredentialsService CredentialsService { get; set; } = default!;
    
    [Inject] private OrchestratorClientService OrchestratorClientService { get; set; } = default!;

    private bool _awsUsernamePasswordDebug = false;
    private string _secretName = string.Empty;
    private string? UrlValue { get; set; }
    private string? BackupUrlValue { get; set; }
    private string? PollingValue { get; set; }
    private string Username { get; set; } = string.Empty;
    private string Password { get; set; } = string.Empty;
    private string PlaceholderText { get; set; } = "No Secret Selected";

    public string SecretName
    {
        get => _secretName;
        set 
        {
            if (string.IsNullOrEmpty(value))
            {
                _secretName = string.Empty;
                Username = string.Empty;
                Password = string.Empty;
            }
            else CheckForCredentials(value);
        }
    }

    protected override void OnInitialized()
    {
        ParserState.OnChange += OnParserStateChanged;
        UpdateFromParserState();
    }
    
    private void OnParserStateChanged()
    {
        UpdateFromParserState();
        OnParserChanged();
        StateHasChanged();
    }
    
    private void UpdateFromParserState()
    {
        // Update UI components with values from ParserState
        UrlValue = ParserState.DownloadUrl;
        BackupUrlValue = ParserState.BackupUrl;
        PollingValue = ParserState.Polling;
        CheckForCredentials(ParserState.SecretName);
    }
    
    private string _parserNameSelection = string.Empty;
    
    private void CheckForCredentials(string parserSecret)
    {
        if (CredentialsService.GetParserSecretNames().Contains(parserSecret))
        {
            _secretName = parserSecret;
            Username = CredentialsService.GetUsername(parserSecret);
            Password = CredentialsService.GetPassword(parserSecret);
        }
        else
        {
            _secretName = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
        }
        
    }
    
    private Task OpenSecretManagementDialog()
    {
        var parameters = new DialogParameters<CredentialsDialog>
        {
            { x => x.Username, Username },
            { x => x.Password, Password },
            
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        return DialogService.ShowAsync<CredentialsDialog>("Secret Management", parameters, options);
    }
    
    private async Task UpdateDialogAsync()
    {
           
        var parameters = new DialogParameters<UpdateDialog>
        {
            { x => x.Parser, ParserState.ParserName},
            { x => x.Url, UrlValue},
            { x => x.BackupUrl, BackupUrlValue},
            { x => x.SecretName, SecretName},
            { x => x.PollingRate, PollingValue}
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialogResult = await (await DialogService.ShowAsync<UpdateDialog>("Update confirmation", parameters, options)).Result;
        var result = dialogResult?.Data as string ?? string.Empty;
        
        if (result == "update")
        {
            // Send request to endpoint
        }
    }
    
    private Task<IEnumerable<string>> Search(string value, CancellationToken token)
    {
        var allParserNames = CredentialsService.GetParserSecretNames();

        // if text is null or empty, return all parser names
        if (string.IsNullOrEmpty(value))
            return Task.FromResult(allParserNames);
    
        // Filter parser names based on the input value
        var filteredParsers = allParserNames
            .Where(key => key.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        
        return Task.FromResult(filteredParsers);
    }
    
    private async Task UpdateParserConfiguration()
    {
        if (string.IsNullOrEmpty(ParserState.ParserName))
        {
            return;
        }
        await OrchestratorClientService.ConfigureDownloader(ParserState.ParserName, UrlValue, BackupUrlValue, "testing", PollingValue);
    }

    private void OnParserChanged()
    {
        if (string.IsNullOrEmpty(ParserState.ParserName)) return;
        if (!CredentialsService.GetParserSecretNames().Contains(ParserState.SecretName))
        {
            Console.WriteLine(ParserState.ParserName +" "+ "" + ParserState.SecretName);
            PlaceholderText = "No Secret Found!";
        }
        else
        {
            Console.WriteLine(ParserState.ParserName +" "+ "" + ParserState.SecretName);
            PlaceholderText = "No Secret Selected";
        }
    }
}