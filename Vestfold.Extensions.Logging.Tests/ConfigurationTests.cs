using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Vestfold.Extensions.Logging;

namespace Tests;

public class ConfigurationTests
{
    [Theory]
    [InlineData("local.settings.json", "Test", "1.2.3", true)]
    [InlineData("local.settings.2.json", "Test", "1.2.3", false)]
    [InlineData("appsettings.json", "Test", "1.2.3", true)]
    [InlineData("appsettings.2.json", "Test", "1.2.3", false)]
    public void Get_correct_configuration_values(string jsonFile, string configAppName, string configVersion, bool expectConfigValues)
    {
        // Arrange
        var config = NormalizeDoubleUnderscoreConfiguration(jsonFile);
        
        // Assert
        Assert.NotNull(config["Serilog:MinimumLevel:Override:Microsoft_Hosting"] ?? config["Serilog:MinimumLevel:Override:Microsoft.Hosting"]);
        Assert.NotNull(config["Serilog:Console:MinimumLevel"]);
        Assert.NotNull(config["BetterStack:SourceToken"]);
        Assert.NotNull(config["BetterStack:Endpoint"]);
        Assert.NotNull(config["BetterStack:MinimumLevel"]);
        Assert.NotNull(config["MicrosoftTeams:WebhookUrl"]);
        Assert.NotNull(config["MicrosoftTeams:UseWorkflows"]);
        Assert.NotNull(config["MicrosoftTeams:TitleTemplate"]);
        Assert.NotNull(config["MicrosoftTeams:MinimumLevel"]);
        Assert.NotNull(config["Serilog:File:Path"]);
        Assert.NotNull(config["Serilog:File:MinimumLevel"]);
        Assert.NotNull(config["Serilog:File:RollingInterval"]);
        
        // Act
        var loggingValues = LoggingExtension.GetLoggingValues(config);

        // Assert
        if (expectConfigValues)
        {
            Assert.Equal(configAppName, loggingValues.AppName);
            Assert.Equal(configVersion, loggingValues.Version);
        }
        else
        {
            Assert.NotNull(loggingValues.AppName);
            Assert.NotEqual(configAppName, loggingValues.AppName);
            Assert.NotNull(loggingValues.Version);
            Assert.NotEqual(configVersion, loggingValues.Version);
        }
        
        Assert.Single(loggingValues.MinimumLevelOverrides);
        Assert.Equal("Microsoft.Hosting", loggingValues.MinimumLevelOverrides.First().key);
        Assert.Equal(LogEventLevel.Error, loggingValues.MinimumLevelOverrides.First().level);
        
        Assert.Equal("https://foo.betterstackdata.com", loggingValues.BetterStack.Endpoint);
        Assert.Equal("Your BetterStack source token", loggingValues.BetterStack.SourceToken);
        Assert.Equal(LogEventLevel.Debug, loggingValues.BetterStack.MinimumLevel);
        
        Assert.Equal("https://outlook.office.com/webhook/...", loggingValues.MicrosoftTeams.WebhookUrl);
        if (jsonFile.EndsWith("2.json"))
        {
            Assert.False(loggingValues.MicrosoftTeams.UseWorkflows);            
        }
        else
        {
            Assert.True(loggingValues.MicrosoftTeams.UseWorkflows);
        }

        Assert.Equal("Test", loggingValues.MicrosoftTeams.TitleTemplate);
        Assert.Equal(LogEventLevel.Error, loggingValues.MicrosoftTeams.MinimumLevel);
        
        Assert.Equal("logs.txt", loggingValues.File.Path);
        Assert.Equal(LogEventLevel.Error, loggingValues.File.MinimumLevel);
        Assert.Equal(RollingInterval.Hour, loggingValues.File.RollingInterval);
        
        Assert.Equal(LogEventLevel.Information, loggingValues.ConsoleMinimumLevel);
    }

    private static IConfiguration NormalizeDoubleUnderscoreConfiguration(string jsonFile)
    {
        if (jsonFile.StartsWith("appsettings"))
        {
            // for appsettings*.json files, we can rely on ConfigurationBuilder to handle nested keys
            return new ConfigurationBuilder()
                .AddJsonFile(jsonFile)
                .Build();
        }
        
        // for local.settings*.json files, we need to manually replace double underscores with colons to create the correct configuration keys
        var jsonDictionary = JsonSerializer.Deserialize<IDictionary<string, string?>>(File.ReadAllText(jsonFile)) ?? throw new InvalidOperationException($"Failed to deserialize '{jsonFile}'.");
        
        var normalizedDictionary = jsonDictionary.ToDictionary(
            kvp => kvp.Key.Replace("__", ":"),
            kvp => kvp.Value
        );
        
        return new ConfigurationBuilder()
            .AddInMemoryCollection(normalizedDictionary)
            .Build();
    }
}