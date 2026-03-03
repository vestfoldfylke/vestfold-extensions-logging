using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

internal record LoggingMicrosoftTeams
{
    internal string? WebhookUrl { get; init; }
    internal bool UseWorkflows { get; init; } = true;
    internal string? TitleTemplate { get; init; }
    internal LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Warning;
}