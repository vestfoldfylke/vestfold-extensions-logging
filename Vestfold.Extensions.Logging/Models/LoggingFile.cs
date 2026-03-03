using Serilog;
using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

public record LoggingFile
{
    public string? Path { get; init; }
    public LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Warning;
    public RollingInterval RollingInterval { get; init; } = RollingInterval.Day;
}