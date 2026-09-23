using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

internal record LoggingMicrosoftTeams : ISerilogSinkConfiguration
{
    internal string? WebhookUrl { get; init; }
    internal bool UseWorkflows { get; init; } = true;
    internal string? TitleTemplate { get; init; }
    internal LogEventLevel MinimumLevel { get; init; }
    
    public bool Enabled => !string.IsNullOrWhiteSpace(WebhookUrl);
    public string[] PropertiesToExclude { get; } = [ Constants.Properties.SecurityAudit ];
    public string[] PropertiesToInclude { get; } = [];
    
    internal static LogEventLevel DefaultMinimumLevel => LogEventLevel.Warning;
}