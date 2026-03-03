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
                .Enrich.FromGlobalLogContext()
                .WriteTo.Console(restrictedToMinimumLevel: loggingValues.ConsoleMinimumLevel);

            foreach (var (key, level) in loggingValues.MinimumLevelOverrides)
            {
                loggerConfiguration.MinimumLevel.Override(key, level);
            }

            if (loggingValues.BetterStack.Endpoint is not null && loggingValues.BetterStack.SourceToken is not null)
            {
                loggerConfiguration
                    .WriteTo.BetterStack(
                        loggingValues.BetterStack.SourceToken,
                        loggingValues.BetterStack.Endpoint,
                        restrictedToMinimumLevel: loggingValues.BetterStack.MinimumLevel);
            }

            if (loggingValues.MicrosoftTeams.WebhookUrl is not null)
            {
                loggerConfiguration
                    .WriteTo.MicrosoftTeams(
                        loggingValues.MicrosoftTeams.WebhookUrl,
                        usePowerAutomateWorkflows: loggingValues.MicrosoftTeams.UseWorkflows,
                        titleTemplate: loggingValues.MicrosoftTeams.TitleTemplate ?? "",
                        restrictedToMinimumLevel: loggingValues.MicrosoftTeams.MinimumLevel);
            }

            if (loggingValues.File.Path is not null)
            {
                loggerConfiguration
                    .WriteTo.File(
                        loggingValues.File.Path,
                        restrictedToMinimumLevel: loggingValues.File.MinimumLevel,
                        rollingInterval: loggingValues.File.RollingInterval,
                        encoding: Encoding.UTF8);
            }
        });
        
        return loggingBuilder;
    }

    public static LoggingValues GetLoggingValues(IConfiguration config)
    {
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

        _ = Enum.TryParse(config[Constants.ConfigurationKeys.ConsoleMinimumLevel], out LogEventLevel consoleMinimumLevel);
        
        var betterStackEndpoint = config[Constants.ConfigurationKeys.BetterStackEndpoint];
        var betterStackSourceToken = config[Constants.ConfigurationKeys.BetterStackSourceToken];

        _ = Enum.TryParse(config[Constants.ConfigurationKeys.BetterStackMinimumLevel], out LogEventLevel betterStackMinimumLevel);
        
        var microsoftTeamsWebhookUrl = config[Constants.ConfigurationKeys.MicrosoftTeamsWebhookUrl];
        var microsoftTeamsTitleTemplate = config[Constants.ConfigurationKeys.MicrosoftTeamsTitleTemplate];
        
        if (!bool.TryParse(config[Constants.ConfigurationKeys.MicrosoftTeamsUseWorkflows], out var microsoftTeamsUseWorkflows))
        {
            microsoftTeamsUseWorkflows = true;
        }

        _ = Enum.TryParse(config[Constants.ConfigurationKeys.MicrosoftTeamsMinimumLevel], out LogEventLevel microsoftTeamsMinimumLevel);
        
        var filePath = config[Constants.ConfigurationKeys.FilePath];

        _ = Enum.TryParse(config[Constants.ConfigurationKeys.FileMinimumLevel], out LogEventLevel fileMinimumLevel);

        _ = Enum.TryParse(config[Constants.ConfigurationKeys.FileRollingInterval], out RollingInterval fileRollingInterval);
        
        return new LoggingValues
        {
            AppName = appName,
            Version = version,
            MinimumLevelOverrides = minimumLevelOverrides,
            BetterStack = new LoggingBetterStack
            {

                Endpoint = betterStackEndpoint,
                SourceToken = betterStackSourceToken,
                MinimumLevel = betterStackMinimumLevel
            },
            MicrosoftTeams = new LoggingMicrosoftTeams
            {
                MinimumLevel = microsoftTeamsMinimumLevel,
                TitleTemplate = microsoftTeamsTitleTemplate,
                UseWorkflows = microsoftTeamsUseWorkflows,
                WebhookUrl = microsoftTeamsWebhookUrl
            },
            File = new LoggingFile
            {
                MinimumLevel = fileMinimumLevel,
                Path = filePath,
                RollingInterval = fileRollingInterval
            },
            ConsoleMinimumLevel = consoleMinimumLevel
        };
    }

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