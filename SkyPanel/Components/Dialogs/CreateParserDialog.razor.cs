using CommonDis.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using SkyPanel.Components.Services;

namespace SkyPanel.Components.Dialogs;

public partial class CreateParserDialog : ComponentBase
{
    public string Parser { get; set; } = "";
    public string ParserName { get; set; }

    public string EmphasizedCenterText { get; set; } = string.Empty;

    public string ConfirmationButtonText { get; set; } = "Ok";

    private bool _isLoading;
    
    public Color Color { get; set; } = Color.Success;

    [CascadingParameter] private IMudDialogInstance? MudDialog { get; set; }
    [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }

    [Inject] private OrchestratorClientService OrchestratorClient { get; set; } = null!;

    private int _activeTabIndex = 1;
    private int SelectedMinutes { get; set; } = 1;
    private int SelectedHours { get; set; } = 6;
    private int SelectedDays { get; set; } = 1;
    private MudForm _form;
    private bool _success;
    private string PollingValue { get; set; } = "0 */6 * * *";
    private string? UrlValue { get; set; }
    private string? BackupUrlValue { get; set; } = "";
    private List<FilterDto> Filters { get; set; } = new List<FilterDto>();
    private string SecretName { get; set; } = string.Empty;

    [Inject] private SecretCredentialsService CredentialsService { get; set; } = default!;

    private void DialogSubmit()
    {
        var data = new DownloaderData()
        {
            DownloadUrl = UrlValue,
            BackUpUrl = BackupUrlValue,
            Parser = Parser,
            Name = ParserName,
            PollingRate = PollingValue,
            Filters = Filters,
            SecretName = SecretName
        };
        if (string.IsNullOrWhiteSpace(UrlValue) || string.IsNullOrWhiteSpace(ParserName))
        {
            Snackbar.Add("Please fill in all required fields. At least Url and Name are required", Severity.Error);
            return;
        }

        if (!VerifyUrl(UrlValue))
        {
            Snackbar.Add("Please enter a valid URL including the protocol", Severity.Error);
            return;
        }

        MudDialog!.Close(DialogResult.Ok(data));
    }

    private bool VerifyUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        try
        {
            var uri = new Uri(url);
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps
                                                   || uri.Scheme == Uri.UriSchemeFtp
                                                   || uri.Scheme == Uri.UriSchemeFtps;
        }
        catch (UriFormatException)
        {
            return false;
        }
    }

    private async Task<IEnumerable<string>> Search(string value, CancellationToken token)
    {
        _isLoading = true;
        StateHasChanged();
        var downloaders = await OrchestratorClient.GetParsers();
        var authenticationState = await authenticationStateTask;
        var user = authenticationState.User;
        _isLoading = false;
        StateHasChanged();
        
        if (!user.IsInRole("Admin")) return [];
        return string.IsNullOrEmpty(value)
            ? downloaders
            : downloaders.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        
    }

    private async Task<IEnumerable<string>> SearchSecrets(string value, CancellationToken token)
    {
        _isLoading = true;
        StateHasChanged();
        var allParserNames = await CredentialsService.GetParserSecretNames();
        _isLoading = false;
        StateHasChanged();
        // if text is null or empty, return all parser names
        if (string.IsNullOrEmpty(value))
            return allParserNames;

        // Filter parser names based on the input value
        return allParserNames
            .Where(key => key.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }

    private void DialogCancel() => MudDialog?.Cancel();

    private void UpdatePollingValue()
    {
        PollingValue = _activeTabIndex switch
        {
            0 => $"*/{SelectedMinutes} * * * *", // Minutes - e.g., "*/5 * * * *"
            1 => $"0 */{SelectedHours} * * *", // Hours - e.g., "0 */2 * * *"
            2 => $"0 0 */{SelectedDays} * *", // Days - e.g., "0 0 */1 * *"
            _ => "*/30 * * * *" // Default value (30 minutes)
        };
    }

    private string ValidateMinutes(int minutes)
    {
        return minutes switch
        {
            < 1 => "Minutes must be at least 1",
            > 59 => "Minutes must be under 60",
            _ => string.Empty
        };
    }

    private string ValidateHours(int hours)
    {
        return hours switch
        {
            < 1 => "Hours must be at least 1",
            > 23 => "Hours must be under 24",
            _ => string.Empty
        };
    }

    private string ValidateDays(int days)
    {
        return days switch
        {
            < 1 => "Days must be at least 1",
            > 7 => "Days must be 7 or less",
            _ => string.Empty
        };
    }

    private async Task OpenFilterDialogAsync()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
        var parameters = new DialogParameters<FilterConfiguration>
        {
            { x => x.SelectedFilters, Filters }
        };
        var dialogResult =
            await DialogService.ShowAsync<FilterConfiguration>("Filter configuration", parameters, options);

        var result = await dialogResult?.Result;
        if (result?.Data is List<FilterDto> updatedFilters)
        {
            Filters = updatedFilters;
        }
    }
    
    private async Task OnFormChanged()
    {
        UpdatePollingValue();
        await _form.Validate();
    }
}