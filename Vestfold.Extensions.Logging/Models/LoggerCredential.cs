namespace Vestfold.Extensions.Logging.Models;

internal record LoggerCredential
{
    internal string? ClientId { get; init; }
    internal string? ClientSecret { get; init; }
    internal string? Endpoint { get; init; }
    internal string? ImmutableId { get; init; }
    internal string? StreamName { get; init; }
    internal string? TenantId { get; init; }
}