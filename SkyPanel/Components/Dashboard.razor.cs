using Microsoft.AspNetCore.Components;

namespace SkyPanel.Components;
using SkyPanel.Components.Services;

public partial class Dashboard : ComponentBase
{
    [Inject] private ParserStateService ParserState { get; set; } = default!;
}