using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

internal record LoggingConsole : ISerilogSinkConfiguration
{
    internal LogEventLevel MinimumLevel { get; init; }

    public bool Enabled => true;
    public string[] PropertiesToExclude { get; } = [];
    public string[] PropertiesToInclude { get; } = [];
    public LogEventLevel? AlwaysIncludeAtOrAboveLevel { get; }

    internal static LogEventLevel DefaultMinimumLevel => LogEventLevel.Debug;
}