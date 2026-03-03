using Serilog;
using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

internal record LoggingFile
{
    internal string? Path { get; init; }
    internal LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Warning;
    internal RollingInterval RollingInterval { get; init; } = RollingInterval.Day;
}