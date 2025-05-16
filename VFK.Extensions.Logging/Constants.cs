using Microsoft.Extensions.Configuration;

namespace VFK.Extensions.Logging;

internal static class Constants
{
    internal static class Properties
    {
        internal const string AppName = "AppName";
        internal const string Version = "Version";
    }
    
    internal class ConfigurationKeys(IConfiguration configuration)
    {
        internal string AppName => GetValue("AppName");
        internal string Version => GetValue("Version");
        
        internal string ConsoleMinimumLevel => GetValue("Serilog:Console:MinimumLevel");
        internal string SerilogMinimumLevelOverrideKey => GetValue("Serilog:MinimumLevel:Override:");
        
        internal string BetterStackSourceToken => GetValue("BetterStack:SourceToken");
        internal string BetterStackEndpoint => GetValue("BetterStack:Endpoint");
        internal string BetterStackMinimumLevel => GetValue("BetterStack:MinimumLevel");
        
        internal string MicrosoftTeamsWebhookUrl => GetValue("MicrosoftTeams:WebhookUrl");
        internal string MicrosoftTeamsUseWorkflows => GetValue("MicrosoftTeams:UseWorkflows");
        internal string MicrosoftTeamsTitleTemplate => GetValue("MicrosoftTeams:TitleTemplate");
        internal string MicrosoftTeamsMinimumLevel => GetValue("MicrosoftTeams:MinimumLevel");

        private string GetValue(string key) => configuration["AZURE_FUNCTIONS_ENVIRONMENT"] is not null
            ? key.Replace(':', '_')
            : key;
    }
}