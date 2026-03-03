using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

public record LoggingMicrosoftTeams
{
    public string? WebhookUrl { get; init; }
    // NOTE: Default value for UseWorkflows is set in LoggingExtensions.GetLoggingValues
    public bool UseWorkflows { get; init; }
    public string? TitleTemplate { get; init; }
    public LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Warning;
}