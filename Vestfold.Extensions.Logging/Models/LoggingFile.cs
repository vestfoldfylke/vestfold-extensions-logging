using Serilog;
using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

internal record LoggingFile : ISerilogSinkConfiguration
{
    internal string? Path { get; init; }
    internal LogEventLevel MinimumLevel { get; init; }
    internal RollingInterval RollingInterval { get; init; }
    
    public bool Enabled => !string.IsNullOrWhiteSpace(Path);
    public string[] PropertiesToExclude { get; } = [];
    public string[] PropertiesToInclude { get; } = [];
    
    internal static LogEventLevel DefaultMinimumLevel => LogEventLevel.Warning;
    internal static RollingInterval DefaultRollingInterval => RollingInterval.Day;
}