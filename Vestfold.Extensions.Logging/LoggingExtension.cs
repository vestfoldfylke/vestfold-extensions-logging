using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AzureLogAnalytics;
using Vestfold.Extensions.Logging.Models;

namespace Vestfold.Extensions.Logging;

public static class LoggingExtension
{
    public static ILoggingBuilder AddVestfoldLogging(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            var config = services.GetRequiredService<IConfiguration>();
            var loggingValues = GetLoggingValues(config);

            loggerConfiguration
                //.ReadFrom.Configuration(config)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty(Constants.Properties.AppName, loggingValues.AppName)
                .Enrich.WithProperty(Constants.Properties.Version, loggingValues.Version)
                .Enrich.FromGlobalLogContext();

            foreach (var (key, level) in loggingValues.MinimumLevelOverrides)
            {
                loggerConfiguration.MinimumLevel.Override(key, level);
            }

            if (loggingValues.AzureLogAnalytics.Enabled)
            {
                loggerConfiguration
                    .WriteTo.Logger(loggerConfig => loggerConfig
                        .Filter.ByIncludingOnly(logEvent => LoggerFilter(logEvent, loggingValues.AzureLogAnalytics.PropertiesToInclude, loggingValues.AzureLogAnalytics.PropertiesToExclude))
                        .WriteTo.AzureLogAnalytics(loggingValues.AzureLogAnalytics.Credential, new ConfigurationSettings
                        {
                            BatchSize = loggingValues.AzureLogAnalytics.BatchSize,
                            BufferSize = loggingValues.AzureLogAnalytics.BufferSize,
                            MinLogLevel = loggingValues.AzureLogAnalytics.MinimumLevel
                        }));
            }

            if (loggingValues.BetterStack.Enabled)
            {
                loggerConfiguration
                    .WriteTo.Logger(loggerConfig => loggerConfig
                        .Filter.ByIncludingOnly(logEvent => LoggerFilter(logEvent, loggingValues.BetterStack.PropertiesToInclude, loggingValues.BetterStack.PropertiesToExclude))
                        .WriteTo.BetterStack(
                            loggingValues.BetterStack.SourceToken!,
                            loggingValues.BetterStack.Endpoint!,
                            restrictedToMinimumLevel: loggingValues.BetterStack.MinimumLevel));
            }

            if (loggingValues.Console.Enabled)
            {
                loggerConfiguration
                    .WriteTo.Logger(loggerConfig => loggerConfig
                        .Filter.ByIncludingOnly(logEvent => LoggerFilter(logEvent, loggingValues.Console.PropertiesToInclude, loggingValues.Console.PropertiesToExclude))
                        .WriteTo.Console(restrictedToMinimumLevel: loggingValues.Console.MinimumLevel));
            }

            if (loggingValues.File.Enabled)
            {
                loggerConfiguration
                    .WriteTo.Logger(loggerConfig => loggerConfig
                        .Filter.ByIncludingOnly(logEvent => LoggerFilter(logEvent, loggingValues.File.PropertiesToInclude, loggingValues.File.PropertiesToExclude))
                        .WriteTo.File(
                            loggingValues.File.Path!,
                            restrictedToMinimumLevel: loggingValues.File.MinimumLevel,
                            rollingInterval: loggingValues.File.RollingInterval,
                            encoding: Encoding.UTF8));
            }

            if (loggingValues.MicrosoftTeams.Enabled)
            {
                loggerConfiguration
                    .WriteTo.Logger(loggerConfig => loggerConfig
                        .Filter.ByIncludingOnly(logEvent => LoggerFilter(logEvent, loggingValues.MicrosoftTeams.PropertiesToInclude, loggingValues.MicrosoftTeams.PropertiesToExclude))
                        .WriteTo.MicrosoftTeams(
                            loggingValues.MicrosoftTeams.WebhookUrl!,
                            usePowerAutomateWorkflows: loggingValues.MicrosoftTeams.UseWorkflows,
                            titleTemplate: loggingValues.MicrosoftTeams.TitleTemplate ?? "",
                            restrictedToMinimumLevel: loggingValues.MicrosoftTeams.MinimumLevel));
            }
        });
        
        return loggingBuilder;
    }

    internal static LoggingValues GetLoggingValues(IConfiguration config)
    {
        // general
        var appName = config[Constants.ConfigurationKeys.AppName]
            ?? Assembly.GetEntryAssembly()?.GetName().Name
            ?? throw new InvalidOperationException($"Missing {Constants.ConfigurationKeys.AppName} in configuration and couldn't get Name from Assembly");
        
        var version = config[Constants.ConfigurationKeys.Version]
                      ?? GetInformationalVersion()
                      ?? throw new InvalidOperationException($"Missing {Constants.ConfigurationKeys.Version} in configuration and couldn't get InformationalVersion in .csproj");
        
        var minimumLevelOverrideKey = Constants.ConfigurationKeys.SerilogMinimumLevelOverrideKey;

        List<(string key, LogEventLevel level)> minimumLevelOverrides = [];
        foreach (var child in config.AsEnumerable().Where(c => c.Key.StartsWith(minimumLevelOverrideKey)))
        {
            var key = Constants.ConfigurationKeys.ConvertAzureFriendlyKeyName(child.Key.Replace(minimumLevelOverrideKey, ""));
            if (!Enum.TryParse(child.Value, out LogEventLevel level))
            {
                throw new InvalidOperationException($"Invalid value for {child.Key} in configuration");
            }

            minimumLevelOverrides.Add((key, level));
        }
        
        // Azure Log Analytics
        var azureLogAnalyticsClientId = config[Constants.ConfigurationKeys.AzureLogAnalyticsClientId];
        var azureLogAnalyticsClientSecret = config[Constants.ConfigurationKeys.AzureLogAnalyticsClientSecret];
        var azureLogAnalyticsEndpoint = config[Constants.ConfigurationKeys.AzureLogAnalyticsEndpoint];
        var azureLogAnalyticsImmutableId = config[Constants.ConfigurationKeys.AzureLogAnalyticsImmutableId];
        var azureLogAnalyticsStreamName = config[Constants.ConfigurationKeys.AzureLogAnalyticsStreamName];
        var azureLogAnalyticsTenantId = config[Constants.ConfigurationKeys.AzureLogAnalyticsTenantId];
        
        _ = int.TryParse(config[Constants.ConfigurationKeys.AzureLogAnalyticsBatchSize], out var azureLogAnalyticsBatchSize);
        _ = int.TryParse(config[Constants.ConfigurationKeys.AzureLogAnalyticsBufferSize], out var azureLogAnalyticsBufferSize);

        var credential = !string.IsNullOrWhiteSpace(azureLogAnalyticsClientId) && !string.IsNullOrWhiteSpace(azureLogAnalyticsClientSecret) && !string.IsNullOrWhiteSpace(azureLogAnalyticsEndpoint) &&
                         !string.IsNullOrWhiteSpace(azureLogAnalyticsImmutableId) && !string.IsNullOrWhiteSpace(azureLogAnalyticsStreamName) && !string.IsNullOrWhiteSpace(azureLogAnalyticsTenantId)
            ? new LoggerCredential
            {
                ClientId = azureLogAnalyticsClientId,
                ClientSecret = azureLogAnalyticsClientSecret,
                Endpoint = azureLogAnalyticsEndpoint,
                ImmutableId = azureLogAnalyticsImmutableId,
                StreamName = azureLogAnalyticsStreamName,
                TenantId = azureLogAnalyticsTenantId
            }
            : null;
        
        _ = Enum.TryParse(config[Constants.ConfigurationKeys.AzureLogAnalyticsMinimumLevel], out LogEventLevel azureLogAnalyticsMinimumLevel);

        // Console
        _ = Enum.TryParse(config[Constants.ConfigurationKeys.ConsoleMinimumLevel], out LogEventLevel consoleMinimumLevel);
        
        // BetterStack
        var betterStackEndpoint = config[Constants.ConfigurationKeys.BetterStackEndpoint];
        var betterStackSourceToken = config[Constants.ConfigurationKeys.BetterStackSourceToken];

        _ = Enum.TryParse(config[Constants.ConfigurationKeys.BetterStackMinimumLevel], out LogEventLevel betterStackMinimumLevel);
        
        // FilePath
        var filePath = config[Constants.ConfigurationKeys.FilePath];

        _ = Enum.TryParse(config[Constants.ConfigurationKeys.FileMinimumLevel], out LogEventLevel fileMinimumLevel);

        _ = Enum.TryParse(config[Constants.ConfigurationKeys.FileRollingInterval], out RollingInterval fileRollingInterval);
        
        // Microsoft Teams
        var microsoftTeamsWebhookUrl = config[Constants.ConfigurationKeys.MicrosoftTeamsWebhookUrl];
        var microsoftTeamsTitleTemplate = config[Constants.ConfigurationKeys.MicrosoftTeamsTitleTemplate];
        
        if (!bool.TryParse(config[Constants.ConfigurationKeys.MicrosoftTeamsUseWorkflows], out var microsoftTeamsUseWorkflows))
        {
            microsoftTeamsUseWorkflows = true;
        }

        _ = Enum.TryParse(config[Constants.ConfigurationKeys.MicrosoftTeamsMinimumLevel], out LogEventLevel microsoftTeamsMinimumLevel);
        
        return new LoggingValues
        {
            AppName = appName,
            Version = version,
            MinimumLevelOverrides = minimumLevelOverrides,
            AzureLogAnalytics = new LoggingAzureLogAnalytics
            {
                Credential = credential,
                BatchSize = azureLogAnalyticsBatchSize,
                BufferSize = azureLogAnalyticsBufferSize,
                MinimumLevel = azureLogAnalyticsMinimumLevel
            },
            BetterStack = new LoggingBetterStack
            {
                Endpoint = betterStackEndpoint,
                SourceToken = betterStackSourceToken,
                MinimumLevel = betterStackMinimumLevel
            },
            Console = new LoggingConsole
            {
                MinimumLevel = consoleMinimumLevel
            },
            File = new LoggingFile
            {
                MinimumLevel = fileMinimumLevel,
                Path = filePath,
                RollingInterval = fileRollingInterval
            },
            MicrosoftTeams = new LoggingMicrosoftTeams
            {
                MinimumLevel = microsoftTeamsMinimumLevel,
                TitleTemplate = microsoftTeamsTitleTemplate,
                UseWorkflows = microsoftTeamsUseWorkflows,
                WebhookUrl = microsoftTeamsWebhookUrl
            }
        };
    }
    
    internal static readonly Func<LogEvent, string[], string[], bool> LoggerFilter = (logEvent, propertiesToInclude, propertiesToExclude) =>
    {
        var include = propertiesToInclude.Length == 0 || logEvent.Properties.Any(property => propertiesToInclude.Contains(property.Key));
        var exclude = logEvent.Properties.Any(property => propertiesToExclude.Contains(property.Key));

        return include && !exclude;
    };

    private static string? GetInformationalVersion()
    {
        var informationalVersion = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (informationalVersion is null)
        {
            return informationalVersion;
        }
        
        var versionParts = informationalVersion.Split("+");
        if (versionParts.Length > 1)
        {
            informationalVersion = versionParts[0];
        }

        return informationalVersion;
    }
}