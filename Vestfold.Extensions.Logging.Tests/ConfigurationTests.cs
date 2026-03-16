using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Vestfold.Extensions.Logging;
using Vestfold.Extensions.Logging.Models;

namespace Tests;

public class ConfigurationTests
{
    [Theory]
    [InlineData("Settings/AzureLogAnalytics/appsettings.json", "AzureLogAnalytics", "1.2.3")]
    [InlineData("Settings/AzureLogAnalytics/local.settings.json", "AzureLogAnalytics", "1.2.3")]
    public void AzureLogAnalytics_Should_Be_Enabled(string jsonFile, string configAppName, string configVersion)
    {
        // Arrange
        var config = NormalizeDoubleUnderscoreConfiguration(jsonFile);
        
        // Assert
        Assert.NotNull(config["Serilog:AzureLogAnalytics:ClientId"]);
        Assert.NotNull(config["Serilog:AzureLogAnalytics:ClientSecret"]);
        Assert.NotNull(config["Serilog:AzureLogAnalytics:Endpoint"]);
        Assert.NotNull(config["Serilog:AzureLogAnalytics:ImmutableId"]);
        Assert.NotNull(config["Serilog:AzureLogAnalytics:StreamName"]);
        Assert.NotNull(config["Serilog:AzureLogAnalytics:TenantId"]);
        Assert.NotNull(config["Serilog:AzureLogAnalytics:MinimumLevel"]);
        Assert.True(int.TryParse(config["Serilog:AzureLogAnalytics:BatchSize"], out var batchSize));
        Assert.True(int.TryParse(config["Serilog:AzureLogAnalytics:BufferSize"], out var bufferSize));
        Assert.NotNull(config["Serilog:MinimumLevel:Override:Microsoft_Hosting"] ?? config["Serilog:MinimumLevel:Override:Microsoft.Hosting"]);
        
        // Act
        var loggingValues = LoggingExtension.GetLoggingValues(config);

        // Assert
        AssertConfigAppNameAndVersion(loggingValues, configAppName, configVersion, true);
        
        Assert.True(loggingValues.AzureLogAnalytics.Enabled);
        Assert.NotNull(loggingValues.AzureLogAnalytics.Credential);
        
        Assert.Equal(config["Serilog:AzureLogAnalytics:ClientId"], loggingValues.AzureLogAnalytics.Credential.ClientId);
        Assert.Equal(config["Serilog:AzureLogAnalytics:ClientSecret"], loggingValues.AzureLogAnalytics.Credential.ClientSecret);
        Assert.Equal(config["Serilog:AzureLogAnalytics:Endpoint"], loggingValues.AzureLogAnalytics.Credential.Endpoint);
        Assert.Equal(config["Serilog:AzureLogAnalytics:ImmutableId"], loggingValues.AzureLogAnalytics.Credential.ImmutableId);
        Assert.Equal(config["Serilog:AzureLogAnalytics:StreamName"], loggingValues.AzureLogAnalytics.Credential.StreamName);
        Assert.Equal(config["Serilog:AzureLogAnalytics:TenantId"], loggingValues.AzureLogAnalytics.Credential.TenantId);
        Assert.Equal(LogEventLevel.Warning, loggingValues.AzureLogAnalytics.MinimumLevel);
        Assert.Equal(batchSize, loggingValues.AzureLogAnalytics.BatchSize);
        Assert.Equal(bufferSize, loggingValues.AzureLogAnalytics.BufferSize);
        
        AssertMinimumLevelOverrides(loggingValues);
    }
    
    [Theory]
    [InlineData("Settings/AzureLogAnalytics/appsettings.2.json", "AzureLogAnalytics", "1.2.3")]
    [InlineData("Settings/AzureLogAnalytics/local.settings.2.json", "AzureLogAnalytics", "1.2.3")]
    public void AzureLogAnalytics_Should_Be_Disabled(string jsonFile, string configAppName, string configVersion)
    {
        // Arrange
        var config = NormalizeDoubleUnderscoreConfiguration(jsonFile);
        
        // Assert
        Assert.Null(config["Serilog:AzureLogAnalytics:ClientId"]);
        Assert.Null(config["Serilog:AzureLogAnalytics:ClientSecret"]);
        Assert.Null(config["Serilog:AzureLogAnalytics:Endpoint"]);
        Assert.NotNull(config["Serilog:AzureLogAnalytics:ImmutableId"]);
        Assert.NotNull(config["Serilog:AzureLogAnalytics:StreamName"]);
        Assert.NotNull(config["Serilog:AzureLogAnalytics:TenantId"]);
        Assert.NotNull(config["Serilog:AzureLogAnalytics:MinimumLevel"]);
        Assert.True(int.TryParse(config["Serilog:AzureLogAnalytics:BatchSize"], out var batchSize));
        Assert.True(int.TryParse(config["Serilog:AzureLogAnalytics:BufferSize"], out var bufferSize));
        Assert.NotNull(config["Serilog:MinimumLevel:Override:Microsoft_Hosting"] ?? config["Serilog:MinimumLevel:Override:Microsoft.Hosting"]);
        
        // Act
        var loggingValues = LoggingExtension.GetLoggingValues(config);

        // Assert
        AssertConfigAppNameAndVersion(loggingValues, configAppName, configVersion, false);
        
        Assert.False(loggingValues.AzureLogAnalytics.Enabled);
        Assert.Null(loggingValues.AzureLogAnalytics.Credential);
        
        Assert.Equal(LogEventLevel.Warning, loggingValues.AzureLogAnalytics.MinimumLevel);
        Assert.Equal(batchSize, loggingValues.AzureLogAnalytics.BatchSize);
        Assert.Equal(bufferSize, loggingValues.AzureLogAnalytics.BufferSize);

        AssertMinimumLevelOverrides(loggingValues);
    }

    [Fact]
    public void AzureLogAnalytics_Should_Only_Allow_SecurityAudit_Property_To_Be_Logged()
    {
        var loggingValue = new LoggingAzureLogAnalytics();
        
        var securityAuditEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test security audit event", []), CreateLogEventProperties((Constants.Properties.SecurityAudit, true)));
        var entraIdEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test Entra ID event", []), CreateLogEventProperties());
        
        Assert.True(LoggingExtension.LoggerFilter(securityAuditEvent, loggingValue.PropertiesToInclude, loggingValue.PropertiesToExclude));
        Assert.False(LoggingExtension.LoggerFilter(entraIdEvent, loggingValue.PropertiesToInclude, loggingValue.PropertiesToExclude));
    }
    
    [Theory]
    [InlineData("Settings/BetterStack/appsettings.json", "BetterStack", "1.2.3")]
    [InlineData("Settings/BetterStack/local.settings.json", "BetterStack", "1.2.3")]
    public void BetterStack_Should_Be_Enabled(string jsonFile, string configAppName, string configVersion)
    {
        // Arrange
        var config = NormalizeDoubleUnderscoreConfiguration(jsonFile);
        
        // Assert
        Assert.NotNull(config["BetterStack:SourceToken"]);
        Assert.NotNull(config["BetterStack:Endpoint"]);
        Assert.NotNull(config["BetterStack:MinimumLevel"]);
        Assert.NotNull(config["Serilog:MinimumLevel:Override:Microsoft_Hosting"] ?? config["Serilog:MinimumLevel:Override:Microsoft.Hosting"]);
        
        // Act
        var loggingValues = LoggingExtension.GetLoggingValues(config);

        // Assert
        AssertConfigAppNameAndVersion(loggingValues, configAppName, configVersion, true);
        
        Assert.True(loggingValues.BetterStack.Enabled);
        
        Assert.Equal(config["BetterStack:SourceToken"], loggingValues.BetterStack.SourceToken);
        Assert.Equal(config["BetterStack:Endpoint"], loggingValues.BetterStack.Endpoint);
        Assert.Equal(LogEventLevel.Debug, loggingValues.BetterStack.MinimumLevel);
        
        AssertMinimumLevelOverrides(loggingValues);
    }
    
    [Theory]
    [InlineData("Settings/BetterStack/appsettings.2.json", "BetterStack", "1.2.3")]
    [InlineData("Settings/BetterStack/local.settings.2.json", "BetterStack", "1.2.3")]
    public void BetterStack_Should_Be_Disabled(string jsonFile, string configAppName, string configVersion)
    {
        // Arrange
        var config = NormalizeDoubleUnderscoreConfiguration(jsonFile);
        
        // Assert
        Assert.Null(config["BetterStack:SourceToken"]);
        Assert.Null(config["BetterStack:Endpoint"]);
        Assert.NotNull(config["BetterStack:MinimumLevel"]);
        Assert.NotNull(config["Serilog:MinimumLevel:Override:Microsoft_Hosting"] ?? config["Serilog:MinimumLevel:Override:Microsoft.Hosting"]);
        
        // Act
        var loggingValues = LoggingExtension.GetLoggingValues(config);

        // Assert
        AssertConfigAppNameAndVersion(loggingValues, configAppName, configVersion, false);
        
        Assert.False(loggingValues.BetterStack.Enabled);
        
        Assert.Equal(LogEventLevel.Debug, loggingValues.BetterStack.MinimumLevel);

        AssertMinimumLevelOverrides(loggingValues);
    }
    
    [Fact]
    public void BetterStack_Should_Allow_All_Properties_To_Be_Logged()
    {
        var loggingValue = new LoggingBetterStack();
        
        var securityAuditEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test security audit event", []), CreateLogEventProperties((Constants.Properties.SecurityAudit, true)));
        var entraIdEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test Entra ID event", []), CreateLogEventProperties());
        
        Assert.True(LoggingExtension.LoggerFilter(securityAuditEvent, loggingValue.PropertiesToInclude, loggingValue.PropertiesToExclude));
        Assert.True(LoggingExtension.LoggerFilter(entraIdEvent, loggingValue.PropertiesToInclude, loggingValue.PropertiesToExclude));
    }
    
    [Theory]
    [InlineData("Settings/Console/appsettings.json", "Console", "1.2.3")]
    [InlineData("Settings/Console/local.settings.json", "Console", "1.2.3")]
    [InlineData("Settings/Console/appsettings.2.json", "Console", "1.2.3")]
    [InlineData("Settings/Console/local.settings.2.json", "Console", "1.2.3")]
    public void Console_Should_Be_Enabled(string jsonFile, string configAppName, string configVersion)
    {
        // Arrange
        var config = NormalizeDoubleUnderscoreConfiguration(jsonFile);
        
        // Assert
        Assert.NotNull(config["Serilog:Console:MinimumLevel"]);
        Assert.NotNull(config["Serilog:MinimumLevel:Override:Microsoft_Hosting"] ?? config["Serilog:MinimumLevel:Override:Microsoft.Hosting"]);
        
        // Act
        var loggingValues = LoggingExtension.GetLoggingValues(config);

        // Assert
        AssertConfigAppNameAndVersion(loggingValues, configAppName, configVersion, !jsonFile.EndsWith("2.json"));
        
        Assert.True(loggingValues.Console.Enabled);
        
        Assert.Equal(LogEventLevel.Information, loggingValues.Console.MinimumLevel);
        
        AssertMinimumLevelOverrides(loggingValues);
    }
    
    [Fact]
    public void Console_Should_Allow_All_Properties_To_Be_Logged()
    {
        var loggingValue = new LoggingConsole();
        
        var securityAuditEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test security audit event", []), CreateLogEventProperties((Constants.Properties.SecurityAudit, true)));
        var entraIdEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test Entra ID event", []), CreateLogEventProperties());
        
        Assert.True(LoggingExtension.LoggerFilter(securityAuditEvent, loggingValue.PropertiesToInclude, loggingValue.PropertiesToExclude));
        Assert.True(LoggingExtension.LoggerFilter(entraIdEvent, loggingValue.PropertiesToInclude, loggingValue.PropertiesToExclude));
    }
    
    [Theory]
    [InlineData("Settings/File/appsettings.json", "File", "1.2.3")]
    [InlineData("Settings/File/local.settings.json", "File", "1.2.3")]
    public void File_Should_Be_Enabled(string jsonFile, string configAppName, string configVersion)
    {
        // Arrange
        var config = NormalizeDoubleUnderscoreConfiguration(jsonFile);
        
        // Assert
        Assert.NotNull(config["Serilog:File:Path"]);
        Assert.NotNull(config["Serilog:File:MinimumLevel"]);
        Assert.NotNull(config["Serilog:File:RollingInterval"]);
        Assert.NotNull(config["Serilog:MinimumLevel:Override:Microsoft_Hosting"] ?? config["Serilog:MinimumLevel:Override:Microsoft.Hosting"]);
        
        // Act
        var loggingValues = LoggingExtension.GetLoggingValues(config);

        // Assert
        AssertConfigAppNameAndVersion(loggingValues, configAppName, configVersion, true);
        
        Assert.True(loggingValues.File.Enabled);
        
        Assert.Equal(config["Serilog:File:Path"], loggingValues.File.Path);
        Assert.Equal(RollingInterval.Hour, loggingValues.File.RollingInterval);
        Assert.Equal(LogEventLevel.Error, loggingValues.File.MinimumLevel);
        
        AssertMinimumLevelOverrides(loggingValues);
    }
    
    [Theory]
    [InlineData("Settings/File/appsettings.2.json", "File", "1.2.3")]
    [InlineData("Settings/File/local.settings.2.json", "File", "1.2.3")]
    public void File_Should_Be_Disabled(string jsonFile, string configAppName, string configVersion)
    {
        // Arrange
        var config = NormalizeDoubleUnderscoreConfiguration(jsonFile);
        
        // Assert
        Assert.Null(config["Serilog:File:Path"]);
        Assert.NotNull(config["Serilog:File:MinimumLevel"]);
        Assert.NotNull(config["Serilog:File:RollingInterval"]);
        Assert.NotNull(config["Serilog:MinimumLevel:Override:Microsoft_Hosting"] ?? config["Serilog:MinimumLevel:Override:Microsoft.Hosting"]);
        
        // Act
        var loggingValues = LoggingExtension.GetLoggingValues(config);

        // Assert
        AssertConfigAppNameAndVersion(loggingValues, configAppName, configVersion, false);
        
        Assert.False(loggingValues.File.Enabled);
        
        Assert.Equal(RollingInterval.Minute, loggingValues.File.RollingInterval);
        Assert.Equal(LogEventLevel.Error, loggingValues.File.MinimumLevel);

        AssertMinimumLevelOverrides(loggingValues);
    }
    
    [Fact]
    public void File_Should_Allow_All_Properties_To_Be_Logged()
    {
        var loggingValue = new LoggingFile();
        
        var securityAuditEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test security audit event", []), CreateLogEventProperties((Constants.Properties.SecurityAudit, true)));
        var entraIdEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test Entra ID event", []), CreateLogEventProperties());
        
        Assert.True(LoggingExtension.LoggerFilter(securityAuditEvent, loggingValue.PropertiesToInclude, loggingValue.PropertiesToExclude));
        Assert.True(LoggingExtension.LoggerFilter(entraIdEvent, loggingValue.PropertiesToInclude, loggingValue.PropertiesToExclude));
    }
    
    [Theory]
    [InlineData("Settings/MicrosoftTeams/appsettings.json", "MicrosoftTeams", "1.2.3")]
    [InlineData("Settings/MicrosoftTeams/local.settings.json", "MicrosoftTeams", "1.2.3")]
    public void MicrosoftTeams_Should_Be_Enabled(string jsonFile, string configAppName, string configVersion)
    {
        // Arrange
        var config = NormalizeDoubleUnderscoreConfiguration(jsonFile);
        
        // Assert
        Assert.NotNull(config["MicrosoftTeams:WebhookUrl"]);
        Assert.NotNull(config["MicrosoftTeams:UseWorkflows"]);
        Assert.NotNull(config["MicrosoftTeams:TitleTemplate"]);
        Assert.NotNull(config["MicrosoftTeams:MinimumLevel"]);
        Assert.NotNull(config["Serilog:MinimumLevel:Override:Microsoft_Hosting"] ?? config["Serilog:MinimumLevel:Override:Microsoft.Hosting"]);
        
        // Act
        var loggingValues = LoggingExtension.GetLoggingValues(config);

        // Assert
        AssertConfigAppNameAndVersion(loggingValues, configAppName, configVersion, true);
        
        Assert.True(loggingValues.MicrosoftTeams.Enabled);
        
        Assert.Equal(config["MicrosoftTeams:WebhookUrl"], loggingValues.MicrosoftTeams.WebhookUrl);
        Assert.True(loggingValues.MicrosoftTeams.UseWorkflows);
        Assert.Equal(config["MicrosoftTeams:TitleTemplate"], loggingValues.MicrosoftTeams.TitleTemplate);
        Assert.Equal(LogEventLevel.Error, loggingValues.MicrosoftTeams.MinimumLevel);
        
        AssertMinimumLevelOverrides(loggingValues);
    }
    
    [Theory]
    [InlineData("Settings/MicrosoftTeams/appsettings.2.json", "MicrosoftTeams", "1.2.3")]
    [InlineData("Settings/MicrosoftTeams/local.settings.2.json", "MicrosoftTeams", "1.2.3")]
    public void MicrosoftTeams_Should_Be_Disabled(string jsonFile, string configAppName, string configVersion)
    {
        // Arrange
        var config = NormalizeDoubleUnderscoreConfiguration(jsonFile);
        
        // Assert
        Assert.Null(config["MicrosoftTeams:WebhookUrl"]);
        Assert.NotNull(config["MicrosoftTeams:UseWorkflows"]);
        Assert.NotNull(config["MicrosoftTeams:TitleTemplate"]);
        Assert.NotNull(config["MicrosoftTeams:MinimumLevel"]);
        Assert.NotNull(config["Serilog:MinimumLevel:Override:Microsoft_Hosting"] ?? config["Serilog:MinimumLevel:Override:Microsoft.Hosting"]);
        
        // Act
        var loggingValues = LoggingExtension.GetLoggingValues(config);

        // Assert
        AssertConfigAppNameAndVersion(loggingValues, configAppName, configVersion, false);
        
        Assert.False(loggingValues.MicrosoftTeams.Enabled);
        
        Assert.False(loggingValues.MicrosoftTeams.UseWorkflows);
        Assert.Equal(config["MicrosoftTeams:TitleTemplate"], loggingValues.MicrosoftTeams.TitleTemplate);
        Assert.Equal(LogEventLevel.Error, loggingValues.MicrosoftTeams.MinimumLevel);

        AssertMinimumLevelOverrides(loggingValues);
    }
    
    [Fact]
    public void MicrosoftTeams_Should_Allow_All_Properties_To_Be_Logged()
    {
        var loggingValue = new LoggingMicrosoftTeams();
        
        var securityAuditEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test security audit event", []), CreateLogEventProperties((Constants.Properties.SecurityAudit, true)));
        var entraIdEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test Entra ID event", []), CreateLogEventProperties());
        
        Assert.False(LoggingExtension.LoggerFilter(securityAuditEvent, loggingValue.PropertiesToInclude, loggingValue.PropertiesToExclude));
        Assert.True(LoggingExtension.LoggerFilter(entraIdEvent, loggingValue.PropertiesToInclude, loggingValue.PropertiesToExclude));
    }

    private static void AssertConfigAppNameAndVersion(LoggingValues loggingValues, string appName, string version, bool expectConfigValues)
    {
        if (expectConfigValues)
        {
            Assert.Equal(appName, loggingValues.AppName);
            Assert.Equal(version, loggingValues.Version);
            return;
        }
        
        Assert.NotNull(loggingValues.AppName);
        Assert.NotEqual(appName, loggingValues.AppName);
        Assert.NotNull(loggingValues.Version);
        Assert.NotEqual(version, loggingValues.Version);
    }

    private static void AssertMinimumLevelOverrides(LoggingValues loggingValues)
    {
        Assert.Single(loggingValues.MinimumLevelOverrides);
        Assert.Equal("Microsoft.Hosting", loggingValues.MinimumLevelOverrides.First().key);
        Assert.Equal(LogEventLevel.Error, loggingValues.MinimumLevelOverrides.First().level);
    }
    
    private static List<LogEventProperty> CreateLogEventProperties(params (string Name, object Value)[] properties)
    {
        List<LogEventProperty> logEventProperties = [
            new LogEventProperty("TestProperty", new ScalarValue("TestValue")),
            new LogEventProperty("UserId", new ScalarValue(Guid.NewGuid())),
            new LogEventProperty("SessionId", new ScalarValue(Guid.NewGuid())),
            new LogEventProperty("RequestPath", new ScalarValue("/api/v1/resource")),
            new LogEventProperty("StatusCode", new ScalarValue(200)),
            new LogEventProperty("ElapsedMs", new ScalarValue(123)),
            new LogEventProperty("IpAddress", new ScalarValue("192.168.1.100")),
            new LogEventProperty("Operation", new ScalarValue("Create")),
            new LogEventProperty("CorrelationId", new ScalarValue(Guid.NewGuid())),
            new LogEventProperty("FeatureFlag", new ScalarValue("BetaEnabled"))
        ];
        
        logEventProperties.AddRange(properties.Select(p => new LogEventProperty(p.Name, new ScalarValue(p.Value))).ToList());
        
        return logEventProperties;
    }

    private static IConfiguration NormalizeDoubleUnderscoreConfiguration(string jsonFile)
    {
        if (jsonFile.Contains("appsettings"))
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