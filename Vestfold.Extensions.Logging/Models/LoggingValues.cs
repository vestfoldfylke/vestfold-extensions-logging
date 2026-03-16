using System.Collections.Generic;
using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

internal record LoggingValues
{
    internal required string AppName { get; init; }
    internal required string Version { get; init; }
    internal IEnumerable<(string key, LogEventLevel level)> MinimumLevelOverrides { get; init; } = [];
    internal required LoggingAzureLogAnalytics AzureLogAnalytics { get; init; }
    internal required LoggingBetterStack BetterStack { get; init; }
    internal required LoggingConsole Console { get; init; }
    internal required LoggingFile File { get; init; }
    internal required LoggingMicrosoftTeams MicrosoftTeams { get; init; }
}