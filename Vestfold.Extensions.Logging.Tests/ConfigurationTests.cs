using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Vestfold.Extensions.Logging;

namespace Tests;

public class ConfigurationTests
{
    [Fact]
    public void AzureAppServicesTest()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json")
            .Build();

        var (appName, minimumLevelOverrides,
            betterStackEndpoint, betterStackSourceToken, betterStackMinimumLevel,
            microsoftTeamsWebhookUrl, microsoftTeamsUseWorkflows, microsoftTeamsTitleTemplate, microsoftTeamsMinimumLevel,
            consoleMinimumLevel, version) = LoggingExtension.GetLoggingValues(config);
        
        Assert.Equal("UseDevelopmentStorage=true", config["AzureWebJobsStorage"]);
        Assert.Equal("dotnet-isolated", config["FUNCTIONS_WORKER_RUNTIME"]);
        
        Assert.Equal("Test", appName);
        Assert.Equal("1.2.3", version);
        
        Assert.Single(minimumLevelOverrides);
        Assert.Equal("Microsoft.Hosting", minimumLevelOverrides.First().key);
        Assert.Equal(LogEventLevel.Error, minimumLevelOverrides.First().level);
        
        Assert.Equal(LogEventLevel.Information, consoleMinimumLevel);
        
        Assert.Equal("https://foo.betterstackdata.com", betterStackEndpoint);
        Assert.Equal("Your BetterStack source token", betterStackSourceToken);
        Assert.Equal(LogEventLevel.Debug, betterStackMinimumLevel);
        
        Assert.Equal("https://outlook.office.com/webhook/...", microsoftTeamsWebhookUrl);
        Assert.True(microsoftTeamsUseWorkflows);
        Assert.Equal("Test", microsoftTeamsTitleTemplate);
        Assert.Equal(LogEventLevel.Error, microsoftTeamsMinimumLevel);
    }
    
    [Fact]
    public void NonAzureTest()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var (appName, minimumLevelOverrides,
            betterStackEndpoint, betterStackSourceToken, betterStackMinimumLevel,
            microsoftTeamsWebhookUrl, microsoftTeamsUseWorkflows, microsoftTeamsTitleTemplate, microsoftTeamsMinimumLevel,
            consoleMinimumLevel, version) = LoggingExtension.GetLoggingValues(config);
        
        Assert.Null(config["AzureWebJobsStorage"]);
        Assert.Null(config["FUNCTIONS_WORKER_RUNTIME"]);
        
        Assert.Equal("Test", appName);
        Assert.Equal("1.2.3", version);
        
        Assert.Single(minimumLevelOverrides);
        Assert.Equal("Microsoft.Hosting", minimumLevelOverrides.First().key);
        Assert.Equal(LogEventLevel.Error, minimumLevelOverrides.First().level);
        
        Assert.Equal(LogEventLevel.Information, consoleMinimumLevel);
        
        Assert.Equal("https://foo.betterstackdata.com", betterStackEndpoint);
        Assert.Equal("Your BetterStack source token", betterStackSourceToken);
        Assert.Equal(LogEventLevel.Debug, betterStackMinimumLevel);
        
        Assert.Equal("https://outlook.office.com/webhook/...", microsoftTeamsWebhookUrl);
        Assert.True(microsoftTeamsUseWorkflows);
        Assert.Equal("Test", microsoftTeamsTitleTemplate);
        Assert.Equal(LogEventLevel.Error, microsoftTeamsMinimumLevel);
    }
}