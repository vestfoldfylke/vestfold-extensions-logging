using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

internal record LoggingBetterStack
{
    internal string? Endpoint { get; init; }
    internal string? SourceToken { get; init; }
    internal LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Information;
}