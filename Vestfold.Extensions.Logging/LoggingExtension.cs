using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Vestfold.Extensions.Logging;

public static class LoggingExtension
{
    public static ILoggingBuilder AddVestfoldLogging(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            var config = services.GetRequiredService<IConfiguration>();
            var (
                appName,
                minimumLevelOverrides,
                betterStackEndpoint,
                betterStackSourceToken,
                betterStackMinimumLevel,
                microsoftTeamsWebhookUrl,
                microsoftTeamsUseWorkflows,
                microsoftTeamsTitleTemplate,
                microsoftTeamsMinimumLevel,
                filePath,
                fileMinimumLevel,
                fileRollingInterval,
                consoleMinimumLevel,
                version) = GetLoggingValues(config);

            loggerConfiguration
                //.ReadFrom.Configuration(config)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty(Constants.Properties.AppName, appName)
                .Enrich.WithProperty(Constants.Properties.Version, version)
                .Enrich.FromGlobalLogContext()
                .WriteTo.Console(restrictedToMinimumLevel: consoleMinimumLevel);

            foreach (var (key, level) in minimumLevelOverrides)
            {
                loggerConfiguration.MinimumLevel.Override(key, level);
            }

            if (betterStackEndpoint is not null && betterStackSourceToken is not null)
            {
                loggerConfiguration
                    .WriteTo.BetterStack(
                        betterStackSourceToken,
                        betterStackEndpoint,
                        restrictedToMinimumLevel: betterStackMinimumLevel);
            }

            if (microsoftTeamsWebhookUrl is not null)
            {
                loggerConfiguration
                    .WriteTo.MicrosoftTeams(
                        microsoftTeamsWebhookUrl,
                        usePowerAutomateWorkflows: microsoftTeamsUseWorkflows,
                        titleTemplate: microsoftTeamsTitleTemplate ?? "",
                        restrictedToMinimumLevel: microsoftTeamsMinimumLevel);
            }

            if (filePath is not null)
            {
                loggerConfiguration
                    .WriteTo.File(
                        filePath,
                        restrictedToMinimumLevel: fileMinimumLevel,
                        rollingInterval: fileRollingInterval,
                        encoding: Encoding.UTF8);
            }
        });
        
        return loggingBuilder;
    }

    public static (string appName, List<(string key, LogEventLevel level)> minimumLevelOverrides,
        string? betterStackEndpoint, string? betterStackSourceToken, LogEventLevel betterStackMinimumLevel,
        string? microsoftTeamsWebhookUrl, bool microsoftTeamsUseWorkflows, string? microsoftTeamsTitleTemplate,
        LogEventLevel microsoftTeamsMinimumLevel, string? filePath, LogEventLevel fileMinimumLevel,
        RollingInterval fileRollingInterval, LogEventLevel consoleMinimumLevel, string version) GetLoggingValues(IConfiguration config)
    {
        Constants.ConfigurationKeys configurationKeys = new(config);
        
        var appName = config[configurationKeys.AppName]
            ?? Assembly.GetEntryAssembly()?.GetName().Name
            ?? throw new InvalidOperationException($"Missing {configurationKeys.AppName} in configuration and couldn't get Name from Assembly");
        
        var version = config[configurationKeys.Version]
                      ?? GetInformationalVersion()
                      ?? throw new InvalidOperationException($"Missing InformationalVersion in .csproj and {configurationKeys.Version} not specified in configuration");
        
        var minimumLevelOverrideKey = configurationKeys.SerilogMinimumLevelOverrideKey;

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
        
        if (!Enum.TryParse(config[configurationKeys.ConsoleMinimumLevel], out LogEventLevel consoleMinimumLevel))
        {
            consoleMinimumLevel = LogEventLevel.Debug;
        }
        
        var betterStackEndpoint = config[configurationKeys.BetterStackEndpoint];
        var betterStackSourceToken = config[configurationKeys.BetterStackSourceToken];
        
        if (!Enum.TryParse(config[configurationKeys.BetterStackMinimumLevel], out LogEventLevel betterStackMinimumLevel))
        {
            betterStackMinimumLevel = LogEventLevel.Information;
        }
        
        var microsoftTeamsWebhookUrl = config[configurationKeys.MicrosoftTeamsWebhookUrl];
        var microsoftTeamsTitleTemplate = config[configurationKeys.MicrosoftTeamsTitleTemplate];
        
        if (!bool.TryParse(config[configurationKeys.MicrosoftTeamsUseWorkflows], out var microsoftTeamsUseWorkflows))
        {
            microsoftTeamsUseWorkflows = true;
        }
        
        if (!Enum.TryParse(config[configurationKeys.MicrosoftTeamsMinimumLevel], out LogEventLevel microsoftTeamsMinimumLevel))
        {
            microsoftTeamsMinimumLevel = LogEventLevel.Warning;
        }
        
        var filePath = config[configurationKeys.FilePath];
        
        if (!Enum.TryParse(config[configurationKeys.FileMinimumLevel], out LogEventLevel fileMinimumLevel))
        {
            fileMinimumLevel = LogEventLevel.Warning;
        }
        
        if (!Enum.TryParse(config[configurationKeys.FileRollingInterval], out RollingInterval fileRollingInterval))
        {
            fileRollingInterval = RollingInterval.Day;
        }
        
        return (
            appName,
            minimumLevelOverrides,
            betterStackEndpoint,
            betterStackSourceToken,
            betterStackMinimumLevel,
            microsoftTeamsWebhookUrl,
            microsoftTeamsUseWorkflows,
            microsoftTeamsTitleTemplate,
            microsoftTeamsMinimumLevel,
            filePath,
            fileMinimumLevel,
            fileRollingInterval,
            consoleMinimumLevel,
            version);
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