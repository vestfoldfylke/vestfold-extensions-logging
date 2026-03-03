using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

public record LoggingBetterStack
{
    public string? Endpoint { get; init; }
    public string? SourceToken { get; init; }
    public LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Information;
}