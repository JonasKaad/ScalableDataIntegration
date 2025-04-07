using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SkyPanel.Utils;

public class SnackbarUtil
{
    public (MarkupString, Severity) FormatConnectionResponse(bool responseValue, string urlType, string url)
    {
        switch (responseValue)
        {
            case false:
                return SnackPop(urlType, url, Severity.Error, "failed connecting");
            case true:
                return SnackPop(urlType, url, Severity.Success, "successfully connected");
        }
    }
    
    public (MarkupString, Severity) SnackPop(string urlType, string url, Severity severity, string message)
    {
        return (new MarkupString($"<div><h3><strong>{urlType}: </strong></h3><h4>[ {url} ] - {message}</h4></div>"), severity);
    }
}