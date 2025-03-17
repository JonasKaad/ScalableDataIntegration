using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkyPanel.Components.Dialogs;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Features;

public partial class ProtocolPanel : ComponentBase, INotifyPropertyChanged
{
    [Inject] private ParserStateService ParserState { get; set; } = default!;
    
    [Inject] private SecretCredentialsService CredentialsService { get; set; } = default!;

    private bool _awsUsernamePasswordDebug = false;
    private string _protoParserName = string.Empty;
    private string _secretName = string.Empty;
    private string? Protocol { get; set; }
    private string? UrlValue { get; set; }
    private int PollingValue { get; set; }
    private string Username { get; set; } = string.Empty;
    private string Password { get; set; } = string.Empty;
    private string PlaceholderText { get; set; } = "No Secret Selected";
    
    public event PropertyChangedEventHandler? PropertyChanged;


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
            else if (CredentialsService.GetParserSecretNames().Contains(value))
            {
                _secretName = value;
                Username = CredentialsService.GetUsername(_secretName);
                Password = CredentialsService.GetPassword(_secretName);
            }
            else
            {
                _secretName = string.Empty;
                Username = string.Empty;
                Password = string.Empty;
            }
        }
    }

    protected override void OnInitialized()
    {
        ParserState.OnChange += OnParserStateChanged;
        _protoParserName = ParserState.ParserName;
        UpdateFromParserState();
    }
    
    private void OnParserStateChanged()
    {
        UpdateFromParserState();
        StateHasChanged();
    }
    
    private void UpdateFromParserState()
    {
        // Update UI components with values from ParserState
        Protocol = ParserState.Protocol;
        UrlValue = ParserState.Url;
        PollingValue = ParserState.Polling;
        ProtoParserName = ParserState.ParserName;
        CheckForCredentials(ProtoParserName);
    }
    
    private string _parserNameSelection = string.Empty;
    
    private void CheckForCredentials(string parserName)
    {
        if (CredentialsService.GetParserSecretNames().Contains(parserName))
        {
            _secretName = parserName;
            Username = CredentialsService.GetUsername(parserName);
            Password = CredentialsService.GetPassword(parserName);
        }
        else
        {
            _secretName = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
        }
        
    }

    private string ProtoParserName
    {
        get => _protoParserName;
        set
        {
            _protoParserName = value;
            OnPropertyChanged();
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

    private void OnPropertyChanged()
    {
        if (string.IsNullOrEmpty(_protoParserName)) return;
        if (!CredentialsService.GetParserSecretNames().Contains(_protoParserName))
        {
            PlaceholderText = "No Secret Found!";
        }
        PlaceholderText = "No Secret Selected";
    }
}