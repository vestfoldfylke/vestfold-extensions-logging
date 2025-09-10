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
        var isAzure = jsonFile.StartsWith("local.settings");
        
        var config = new ConfigurationBuilder()
            .AddJsonFile(jsonFile)
            .Build();
        
        // Act
        var (appName, minimumLevelOverrides,
            betterStackEndpoint, betterStackSourceToken, betterStackMinimumLevel,
            microsoftTeamsWebhookUrl, microsoftTeamsUseWorkflows, microsoftTeamsTitleTemplate, microsoftTeamsMinimumLevel,
            filePath, fileMinimumLevel, fileRollingInterval,
            consoleMinimumLevel, version) = LoggingExtension.GetLoggingValues(config);

        // Assert
        if (isAzure)
        {
            Assert.Equal("UseDevelopmentStorage=true", config["AzureWebJobsStorage"]);
            Assert.Equal("dotnet-isolated", config["FUNCTIONS_WORKER_RUNTIME"]);
        }
        else
        {
            Assert.Null(config["AzureWebJobsStorage"]);
            Assert.Null(config["FUNCTIONS_WORKER_RUNTIME"]);
        }

        if (expectConfigValues)
        {
            Assert.Equal(configAppName, appName);
            Assert.Equal(configVersion, version);
        }
        else
        {
            Assert.NotNull(appName);
            Assert.NotEqual(configAppName, appName);
            Assert.NotNull(version);
            Assert.NotEqual(configVersion, version);
        }
        
        Assert.Single(minimumLevelOverrides);
        Assert.Equal("Microsoft.Hosting", minimumLevelOverrides.First().key);
        Assert.Equal(LogEventLevel.Error, minimumLevelOverrides.First().level);
        
        Assert.Equal("https://foo.betterstackdata.com", betterStackEndpoint);
        Assert.Equal("Your BetterStack source token", betterStackSourceToken);
        Assert.Equal(LogEventLevel.Debug, betterStackMinimumLevel);
        
        Assert.Equal("https://outlook.office.com/webhook/...", microsoftTeamsWebhookUrl);
        Assert.True(microsoftTeamsUseWorkflows);
        Assert.Equal("Test", microsoftTeamsTitleTemplate);
        Assert.Equal(LogEventLevel.Error, microsoftTeamsMinimumLevel);
        
        Assert.Equal("logs.txt", filePath);
        Assert.Equal(LogEventLevel.Error, fileMinimumLevel);
        Assert.Equal(RollingInterval.Hour, fileRollingInterval);
        
        Assert.Equal(LogEventLevel.Information, consoleMinimumLevel);
    }
}