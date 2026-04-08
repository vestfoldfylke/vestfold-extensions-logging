using Serilog;
using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

internal record LoggingFile : ISerilogSinkConfiguration
{
    internal string? Path { get; init; }
    internal LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Warning;
    internal RollingInterval RollingInterval { get; init; } = RollingInterval.Day;
    
    public bool Enabled => !string.IsNullOrWhiteSpace(Path);
    public string[] PropertiesToExclude { get; } = [];
    public string[] PropertiesToInclude { get; } = [];
}