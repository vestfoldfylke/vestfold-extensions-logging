using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

internal record LoggingBetterStack : ISerilogSinkConfiguration
{
    internal string? Endpoint { get; init; }
    internal string? SourceToken { get; init; }
    internal LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Information;
    
    public bool Enabled => !string.IsNullOrWhiteSpace(Endpoint) && !string.IsNullOrWhiteSpace(SourceToken);
    public string[] PropertiesToExclude { get; } = [];
    public string[] PropertiesToInclude { get; } = [];
}