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
    private string Username { get; set; } = string.Empty;
    private string Password { get; set; } = string.Empty;
    private string PlaceholderText { get; set; } = "No Secret Selected";
    
    // Properties for the polling frequency selector
    private int _activeTabIndex = 0;
    private int SelectedMinutes { get; set; } = 0;
    private int SelectedHours { get; set; } = 0;
    private int SelectedDays { get; set; } = 0;

    private MudForm _form;
    private bool _success;

    // Convert the selected tab and value to a polling rate string
    private void UpdatePollingValue()
    {
        PollingValue = _activeTabIndex switch
        {
            0 => $"*/{ SelectedMinutes } * * * *", // Minutes - e.g., "*/5 * * * *"
            1 => $"0 */{ SelectedHours } * * *",   // Hours - e.g., "0 */2 * * *"
            2 => $"0 0 */{ SelectedDays } * *",    // Days - e.g., "0 0 */1 * *"
            _ => "*/30 * * * *"                   // Default value (30 minutes)
        };
    }

    private bool IsValid()
    {
        return !_success && ParserState.ParserIsNotSelected();
    }

    private string ValidateMinutes(int minutes)
    {
        switch (minutes)
        {
            case < 1:
                return "Minutes must be at least 1";
            case > 59:
                return "Minutes must be under 60";
            default:
                return string.Empty;
        }
    }
    private string ValidateHours(int hours)
    {
        if (hours < 1)
            return "Hours must be at least 1";
        if (hours > 23)
            return "Hours must be under 24";
        return string.Empty;
    }

    private string ValidateDays(int days)
    {
        if (days < 1)
            return "Days must be at least 1";
        if (days > 7)
            return "Days must be 7 or less";
        return string.Empty;
    }

    private void ParsePollingValue(string pollingValue)
    {
        if (string.IsNullOrEmpty(pollingValue))
        {
            _activeTabIndex = 0; 
            SelectedMinutes = 0;
            SelectedHours = 1;
            SelectedDays = 0;
            PollingValue = string.Empty;
            return;
        }
        // Check if it's a cron expression (e.g., "*/5 * * * *")
        if (pollingValue.Contains('*'))
        {
            var parts = pollingValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Handle minute-based cron (*/n * * * *)
            if (parts.Length > 0 && parts[0].StartsWith("*/"))
            {
                if (int.TryParse(parts[0].Substring(2), out int minutes))
                {
                    _activeTabIndex = 0;
                    SelectedMinutes = minutes;
                    PollingValue = pollingValue;
                    return;
                }
            }

            // Handle hour-based cron (0 */n * * *)
            if (parts.Length > 1 && parts[1].StartsWith("*/"))
            {
                if (int.TryParse(parts[1].Substring(2), out int hours))
                {
                    _activeTabIndex = 1;
                    SelectedHours = hours;
                    PollingValue = pollingValue;
                    return;
                }
            }

            // Handle day-based cron (0 0 */n * *)
            if (parts.Length > 2 && parts[2].StartsWith("*/"))
            {
                if (int.TryParse(parts[2].Substring(2), out int days))
                {
                    _activeTabIndex = 2;
                    SelectedDays = days;
                    PollingValue = pollingValue;
                    return;
                }
            }

            // Default if cron format wasn't recognized
            _activeTabIndex = 0;
            SelectedMinutes = 0;
            PollingValue = "* * * * *";
            return;
        }

        // Handle the simplified format (e.g., "5m", "1h", "1d")
        if (int.TryParse(string.Join("", pollingValue.TakeWhile(char.IsDigit)), out int value))
        {
            char unit = pollingValue.FirstOrDefault(c => char.IsLetter(c));

            switch (unit)
            {
                case 'm':
                    _activeTabIndex = 0;
                    SelectedMinutes = value;
                    PollingValue = $"*/{ value } * * * *";
                    break;
                case 'h':
                    _activeTabIndex = 1;
                    SelectedHours = value;
                    PollingValue = $"0 */{ value } * * *";
                    break;
                case 'd':
                    _activeTabIndex = 2;
                    SelectedDays = value;
                    PollingValue = $"0 0 */{ value } * *";
                    break;
                default:
                    _activeTabIndex = 0;
                    SelectedMinutes = 0;
                    PollingValue = "* * * * *";
                    break;
            }
        }
        else
        {
            // Default fallback
            _activeTabIndex = 0;
            SelectedMinutes = 0;
            PollingValue = "* * * * *";
        }
    }

    private string PollingValue { get; set; } = string.Empty;
    
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
        //PollingValue = ParserState.Polling;
        if (ParserState.ParserIsNotSelected())
        {
            // Reset to default values when no parser is selected
            _activeTabIndex = 2;
            SelectedMinutes = 1;
            SelectedHours = 1; 
            SelectedDays = 0;
            PollingValue = string.Empty;
        }
        else
        {
            // Parse polling value when a parser is selected
            ParsePollingValue(ParserState.Polling);
        }
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
           await UpdateParserConfiguration();
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

        var urlValueToSend = " ";
        var backupUrlValueToSend = " ";
        var secretNameToSend = " ";
        var pollingRateToSend = " ";
        
        // Check if parser values have changed. If not send a string with a space: " "
        if (UrlValue != null)
        {
            urlValueToSend = string.Compare(ParserState.DownloadUrl, UrlValue, StringComparison.OrdinalIgnoreCase) == 0 ? " " : UrlValue;
        }
        
        if (BackupUrlValue != null)
        {
            backupUrlValueToSend = string.Compare(ParserState.BackupUrl, BackupUrlValue, StringComparison.OrdinalIgnoreCase) == 0 ? " " : BackupUrlValue;
        }
        
        secretNameToSend = string.Compare(ParserState.SecretName, SecretName, StringComparison.OrdinalIgnoreCase) == 0 ? " " : SecretName;
        
        pollingRateToSend = PollingValue; //string.Compare(ParserState.Polling, PollingValue, StringComparison.OrdinalIgnoreCase) == 0 ? " " : PollingValue;
        
        await OrchestratorClientService.ConfigureDownloader(ParserState.ParserName, 
            urlValueToSend, backupUrlValueToSend, secretNameToSend, pollingRateToSend);
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