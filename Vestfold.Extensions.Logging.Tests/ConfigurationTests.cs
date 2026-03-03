using System.Linq;
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
        var loggingValues = LoggingExtension.GetLoggingValues(config);

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
        Assert.True(loggingValues.MicrosoftTeams.UseWorkflows);
        Assert.Equal("Test", loggingValues.MicrosoftTeams.TitleTemplate);
        Assert.Equal(LogEventLevel.Error, loggingValues.MicrosoftTeams.MinimumLevel);
        
        Assert.Equal("logs.txt", loggingValues.File.Path);
        Assert.Equal(LogEventLevel.Error, loggingValues.File.MinimumLevel);
        Assert.Equal(RollingInterval.Hour, loggingValues.File.RollingInterval);
        
        Assert.Equal(LogEventLevel.Information, loggingValues.ConsoleMinimumLevel);
    }
}