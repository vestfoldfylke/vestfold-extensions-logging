using Serilog.Events;

namespace Vestfold.Extensions.Logging.Models;

internal record LoggingAzureLogAnalytics : ISerilogSinkConfiguration
{
    internal LoggerCredential? Credential { get; init; }
    internal int BatchSize { get; init; } = 100; // Seems to be the default value in the sink
    internal int BufferSize { get; init; } = 5000; // Seems to be the default value in the sink
    internal int PeriodSeconds { get; init; } = 30;
    internal LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Information;

    public bool Enabled => Credential is { ClientId: not null, ClientSecret: not null, Endpoint: not null, ImmutableId: not null, StreamName: not null, TenantId: not null };
    public string[] PropertiesToExclude { get; } = [];
    public string[] PropertiesToInclude { get; } = [ Constants.Properties.SecurityAudit ];
}