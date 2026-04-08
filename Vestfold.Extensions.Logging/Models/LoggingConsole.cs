using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

internal record LoggingConsole : ISerilogSinkConfiguration
{
    internal LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Debug;

    public bool Enabled => true;
    public string[] PropertiesToExclude { get; } = [];
    public string[] PropertiesToInclude { get; } = [];
}