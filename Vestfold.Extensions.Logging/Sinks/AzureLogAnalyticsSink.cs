using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Ingestion;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Serilog.Debugging;

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

        try
        {
            await _client.UploadAsync(_ruleId, _streamName,
                RequestContent.Create(BinaryData.FromObjectAsJson(entries)));
        }
        catch (RequestFailedException rex)
        {
            var data = rex.Data.Count > 0
                ? $"Data: {System.Text.Json.JsonSerializer.Serialize(rex.Data)}"
                : "";

            SelfLog.WriteLine($"-----------------------------------------------------------------\n" +
                              $"Azure Log Analytics Sink upload failed for {entries.Count} log events!\n" +
                              $"ErrorCode: {rex.ErrorCode}\n" +
                              $"Status: {rex.Status}\n" +
                              $"Message: {rex.Message}\n" +
                              $"StackTrace: {rex.StackTrace}\n" +
                              $"{data}\n" +
                              $"-----------------------------------------------------------------");
        }
        catch (Exception ex)
        {
            var data = ex.Data.Count > 0
                ? $"Data: {System.Text.Json.JsonSerializer.Serialize(ex.Data)}"
                : "";

            SelfLog.WriteLine($"-----------------------------------------------------------------\n" +
                              $"Azure Log Analytics Sink upload failed for {entries.Count}log events!\n" +
                              $"Message: {ex.Message}\n" +
                              $"StackTrace: {ex.StackTrace}\n" +
                              $"{data}\n" +
                              $"-----------------------------------------------------------------");
        }
    }

    public Task OnEmptyBatchAsync() => Task.CompletedTask;

    private static object? SimplifyValue(LogEventPropertyValue value) => value switch
    {
        ScalarValue scalar => scalar.Value,
        _ => value.ToString()
    };
}