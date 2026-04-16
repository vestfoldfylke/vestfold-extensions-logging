using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Ingestion;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vestfold.Extensions.Logging.Sinks;

internal sealed class AzureLogAnalyticsSink : IBatchedLogEventSink
{
    private readonly LogsIngestionClient _client;
    private readonly string _ruleId;
    private readonly string _streamName;

    internal AzureLogAnalyticsSink(string endpoint, string immutableId, string streamName, string tenantId, string clientId, string clientSecret)
    {
        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        _client = new LogsIngestionClient(new Uri(endpoint), credential);
        _ruleId = immutableId;
        _streamName = streamName;
    }

    public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> batch)
    {
        var entries = batch.Select(logEvent =>
        {
            var dict = new Dictionary<string, object?>
            {
                ["TimeGenerated"] = logEvent.Timestamp.UtcDateTime,
                ["Level"] = logEvent.Level.ToString(),
                ["Message"] = logEvent.RenderMessage(),
                ["Exception"] = logEvent.Exception?.ToString()
            };

            foreach (var prop in logEvent.Properties)
            {
                dict[prop.Key] = SimplifyValue(prop.Value);
            }

            return dict;
        }).ToList();

        await _client.UploadAsync(_ruleId, _streamName,
            RequestContent.Create(BinaryData.FromObjectAsJson(entries)));
    }

    public Task OnEmptyBatchAsync() => Task.CompletedTask;

    private static object? SimplifyValue(LogEventPropertyValue value) => value switch
    {
        ScalarValue scalar => scalar.Value,
        _ => value.ToString()
    };
}