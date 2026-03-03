using System.Collections.Generic;
using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

public record LoggingValues
{
    public required string AppName { get; init; }
    public required string Version { get; init; }
    public IEnumerable<(string key, LogEventLevel level)> MinimumLevelOverrides { get; init; } = [];
    public required LoggingBetterStack BetterStack { get; init; }
    public required LoggingMicrosoftTeams MicrosoftTeams { get; init; }
    public required LoggingFile File { get; init; }
    public LogEventLevel ConsoleMinimumLevel { get; init; } = LogEventLevel.Debug;
}