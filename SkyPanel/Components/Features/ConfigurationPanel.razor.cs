using System.ComponentModel;
using CommonDis.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using MudBlazor.Charts;
using SkyPanel.Components.Dialogs;
using SkyPanel.Components.Services;
using SkyPanel.Utils;

namespace SkyPanel.Components.Features;

public partial class ConfigurationPanel : ComponentBase
{
    private readonly ILogger<ConfigurationPanel> _logger;
    
    public ConfigurationPanel (ILogger<ConfigurationPanel> logger)
    {
        _logger = logger;
    }
    private SnackbarUtil SnackbarUtil { get; set; } = new();

    [CascadingParameter] 
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    [Inject] private ParserStateService ParserState { get; set; } = default!;
    
    [Inject] private SecretCredentialsService CredentialsService { get; set; } = default!;
    
    [Inject] private OrchestratorClientService OrchestratorClientService { get; set; } = default!;

    private string _secretName = string.Empty;
    private string? UrlValue { get; set; }
    private string? BackupUrlValue { get; set; }
    private string TokenName { get; set; } = string.Empty;
    private string Token { get; set; } = string.Empty;
    private string PlaceholderText { get; set; } = "No Secret Selected";
    
    private List<FilterDto> CurrentFilters { get; set; } = new List<FilterDto>();
    
    // Properties for the polling frequency selector
    private int _activeTabIndex = 0;
    private int SelectedMinutes { get; set; } = 0;
    private int SelectedHours { get; set; } = 0;
    private int SelectedDays { get; set; } = 0;

    private MudForm _form;
    private bool _success;
    private bool _isSyncing;
    private bool _isTestingConnection;

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
            SelectedDays = 1;
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
                TokenName = string.Empty;
                Token = string.Empty;
            }
            else
            {
                CheckForCredentials(value).Wait();
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        ParserState.OnChange += OnParserStateChanged;
        await UpdateFromParserState();
    }
    
    private void OnParserStateChanged()
    {
        _ = UpdateFromParserState();
        _ = OnParserChanged();
    }
    
    private async Task UpdateFromParserState()
    {
        // Update UI components with values from ParserState
        UrlValue = ParserState.DownloadUrl;
        BackupUrlValue = ParserState.BackupUrl;
        CurrentFilters = ParserState.Filters.Select(f => new FilterDto
        {
            Name = f.Name,
            Parameters = new Dictionary<string, string>(f.Parameters)
        }).ToList();
        if (ParserState.ParserIsNotSelected())
        {
            // Reset to default values when no parser is selected
            _activeTabIndex = 2;
            SelectedMinutes = 1;
            SelectedHours = 1; 
            SelectedDays = 1;
            PollingValue = string.Empty;
        }
        else
        {
            // Parse polling value when a parser is selected
            ParsePollingValue(ParserState.Polling);
        }
        await Task.Delay(10);
        await _form.Validate();
        
        _isLoading = true;
        StateHasChanged();
    
        try
        {
            await CheckForCredentials(ParserState.SecretName);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
        
    }
    
    private string _parserNameSelection = string.Empty;
    
    private async Task OpenSecretManagementDialog()
    {
        var parameters = new DialogParameters<CredentialsDialog>
        {
            { x => x.TokenName, TokenName },
            { x => x.Token, Token },
            { x => x.SecretName, SecretName },
            
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        
        var dialogResult = await (await DialogService.ShowAsync<CredentialsDialog>("Secret Management", parameters, options)).Result;

        if (dialogResult?.Data is DisSecret result)
        {
            // Update the secret in the backend
            await CredentialsService.UpdateSecretAsync(SecretName, result);
            
            _isSyncing = true;
            StateHasChanged();
            await CheckForCredentials(SecretName);
            _isSyncing = false;
            StateHasChanged();
            Snackbar.Add("Secret updated successfully", Severity.Success);
        }
    }
    
    private bool SecretIsNotChosen => string.IsNullOrEmpty(SecretName);

    private async Task OpenFilterDialogAsync()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
        var parameters = new DialogParameters<FilterConfiguration>
        {
            { x => x.SelectedFilters, CurrentFilters }
        };
        var dialogResult = await DialogService.ShowAsync<FilterConfiguration>("Filter configuration", parameters, options);
        
        var result = await dialogResult?.Result;
        if (result?.Data is List<FilterDto> updatedFilters)
        {
            CurrentFilters = updatedFilters;
            Snackbar.Add($"Filters saved for {ParserState.ParserName}", Severity.Success);
        }
    }
    
    private async Task UpdateDialogAsync()
    {
           
        var parameters = new DialogParameters<UpdateDialog>
        {
            { x => x.Parser, ParserState.ParserName},
            { x => x.Url, UrlValue},
            { x => x.BackupUrl, BackupUrlValue},
            { x => x.SecretName, SecretName},
            { x => x.PollingRate, PollingValue},
            { x => x.Filters, CurrentFilters}
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialogResult = await (await DialogService.ShowAsync<UpdateDialog>("Update confirmation", parameters, options)).Result;
        var result = dialogResult?.Data as string ?? string.Empty;
        
        if (result == "update")
        {
            await SyncParserState();
        }
    }

    private async Task SyncParserState()
    {
        _isSyncing = true;          
        StateHasChanged();
        
        await UpdateParserConfiguration();
        var updatedConfig = await OrchestratorClientService.GetDownloaderConfiguration(ParserState.ParserName);
        ParserState.SetParser(updatedConfig);
        await UpdateFromParserState();
        
        _isSyncing = false;
        StateHasChanged();
    }
    
    private async Task<IEnumerable<string>> Search(string value, CancellationToken token)
    {
        var allParserNames = await CredentialsService.GetParserSecretNames();

        // if text is null or empty, return all parser names
        if (string.IsNullOrEmpty(value))
            return allParserNames;
    
        // Filter parser names based on the input value
        return allParserNames
            .Where(key => key.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }
    
    private async Task UpdateParserConfiguration()
    {
        if (string.IsNullOrEmpty(ParserState.ParserName))
        {
            return;
        }

        var urlValueToSend = "";
        var backupUrlValueToSend = "";
        var secretNameToSend = "";
        var pollingRateToSend = "";
        
        // Track what changes were made for audit logging
        var changes = new List<string>();
        
        // Check if parser values have changed. If not send a string with a space: " "
        if (string.IsNullOrEmpty(UrlValue))
        {
            urlValueToSend = " ";
        }
        else
        {
            urlValueToSend = string.Compare(ParserState.DownloadUrl, UrlValue, StringComparison.OrdinalIgnoreCase) == 0 ? "" : UrlValue;
            if (!string.IsNullOrEmpty(urlValueToSend))
            {
                changes.Add($"URL changed from '{ParserState.DownloadUrl ?? "empty"}' to '{UrlValue}'");
            }
        } 
        
        if (string.IsNullOrEmpty(BackupUrlValue))
        {
            backupUrlValueToSend = " ";
        }
        else
        {
            backupUrlValueToSend = string.Compare(ParserState.BackupUrl, BackupUrlValue, StringComparison.OrdinalIgnoreCase) == 0 ? "" : BackupUrlValue;
            if (!string.IsNullOrEmpty(backupUrlValueToSend))
            {
                changes.Add($"Backup URL changed from '{ParserState.BackupUrl ?? "empty"}' to '{BackupUrlValue}'");
            }
        }

        if (string.IsNullOrEmpty(SecretName))
        {
            secretNameToSend = " ";
        }
        else
        {
            secretNameToSend = string.Compare(ParserState.SecretName, SecretName, StringComparison.OrdinalIgnoreCase) == 0 ? "" : SecretName;
            if (!string.IsNullOrEmpty(secretNameToSend))
            {
                changes.Add($"Secret name changed from '{ParserState.SecretName ?? "empty"}' to '{SecretName}'");
            }
        }
        
        pollingRateToSend = PollingValue;
        if (!string.Equals(ParserState.Polling, pollingRateToSend, StringComparison.OrdinalIgnoreCase))
        {
            changes.Add($"Polling rate changed from '{ParserState.Polling ?? "empty"}' to '{pollingRateToSend}'");
        }

        var filters = CurrentFilters;
        if (!AreFiltersEqual(filters, ParserState.Filters))
        {
            var oldFilterNames = string.Join(", ",CurrentFilters.Select(f => f.Name));
            var newFilterNames = string.Join(", ",filters.Select(f => f.Name));
            changes.Add($"Filters changed from '{oldFilterNames}' to '{newFilterNames}'");
        }
        
        var response = await OrchestratorClientService.ConfigureDownloader(ParserState.ParserName, 
            urlValueToSend, backupUrlValueToSend, secretNameToSend, pollingRateToSend, filters);
        if (response)
        {
            var authState = await AuthenticationStateTask;
            var authUser = authState.User;
            var user = RoleUtil.GetUserEmail(authUser);
            
            // Summary entry
            _logger.LogInformation( "[AUDIT] {User} updated configuration for {Parser}", user, ParserState.ParserName);   
            
            // Log detailed changes
            if (changes.Count > 0)
            {
                foreach (var change in changes)
                {
                    _logger.LogInformation("[AUDIT] {User} for {Parser}: {Change}", user, ParserState.ParserName, change);
                }
            }
            else
            {
                _logger.LogInformation("[AUDIT] {User} submitted configuration update for {Parser} but no values were changed", 
                    user, ParserState.ParserName);
            }
            Snackbar.Add("Successfully updated configuration", Severity.Success);
            _ = UpdateFromParserState();
        }
        else
        {
            Snackbar.Add("Failed updating configuration", Severity.Error);
        }
    }
    
    private bool AreFiltersEqual(List<FilterDto> list1, List<FilterDto> list2)
    {
        if (list1.Count != list2.Count)
            return false;
        
        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i].Name != list2[i].Name)
                return false;
            
            // Compare parameters if needed
            var params1 = list1[i].Parameters;
            var params2 = list2[i].Parameters;
        
            if (params1.Count != params2.Count)
                return false;
            
            foreach (var key in params1.Keys)
            {
                if (!params2.ContainsKey(key) || params1[key] != params2[key])
                    return false;
            }
        }
    
        return true;
    }

    private async Task TestConnection()
    {
        if (string.IsNullOrEmpty(ParserState.ParserName))
        {
            return;
        }
        
        _isTestingConnection = true;
        StateHasChanged();

        try
        {
            var urlValueToSend = "";
            var backupUrlValueToSend = "";
            var secretNameToSend = "";
            var pollingRateToSend = "";
            
            // Check if parser values have changed. If not send a string without a space: ""
            if (UrlValue != null)
            {
                urlValueToSend = UrlValue;
            }
            
            if (BackupUrlValue != null)
            {
                backupUrlValueToSend = BackupUrlValue;
            }
            
            secretNameToSend = SecretName;
            
            pollingRateToSend = PollingValue; 
            
            var filters = ParserState.Filters;
            
            var response = await OrchestratorClientService.TestConnection(ParserState.ParserName, 
                urlValueToSend, backupUrlValueToSend, secretNameToSend, pollingRateToSend, filters);
            
            switch (response.Count)
            {
                case 0:
                    Snackbar.Add("Failed getting a response from downloader", Severity.Error);
                    break;
                case 1:
                    Snackbar.Add(SnackbarUtil.FormatConnectionResponse(response.FirstOrDefault(), "URL", UrlValue).Item1, 
                        SnackbarUtil.FormatConnectionResponse(response.FirstOrDefault(), "URL", UrlValue).Item2);
                    break;
                case 2:
                    Snackbar.Add(SnackbarUtil.FormatConnectionResponse(response[0], "URL", UrlValue).Item1, 
                        SnackbarUtil.FormatConnectionResponse(response[0], "URL", UrlValue).Item2);
                    
                    Snackbar.Add(SnackbarUtil.FormatConnectionResponse(response[1], "BackupURL", BackupUrlValue).Item1, 
                        SnackbarUtil.FormatConnectionResponse(response[1], "BackupURL", BackupUrlValue).Item2);
                    break;
                default:
                    Snackbar.Add("Failed getting a correct response from downloader", Severity.Error);
                    break;
            }
        }
        finally
        {
            _isTestingConnection = false;
            StateHasChanged();
        }
    }


    private bool _waitingForSecret = false;
    private bool _isLoading = false;
    
    private async Task CheckForCredentials(string parserSecret)
    {
        _isLoading = true;
        StateHasChanged();
    
        try 
        {
            var secret = await CredentialsService.GetSecret(parserSecret);
            _secretName = parserSecret ?? "";
            TokenName = secret.TokenName ?? "";
            Token = secret.Token ?? "";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
    
    
    private async Task OnParserChanged()
    {
        if (string.IsNullOrEmpty(ParserState.ParserName)) return;
        _isLoading = true;
        StateHasChanged();
        try
        {
            var secretExists = await CredentialsService.HasSecret(ParserState.SecretName);
            _waitingForSecret = false;
            PlaceholderText = !secretExists ? "No Secret Found!" : "No Secret Selected";

            await _form.Validate();
        }
        finally
        {
            _isLoading = false; 
            StateHasChanged();
        }
    }
    private async Task OnFormChanged()
    {
        UpdatePollingValue();
        await _form.Validate();
    }
}