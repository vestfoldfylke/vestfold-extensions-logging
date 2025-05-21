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
        internal string AppName => GetKeyName("AppName");
        internal string Version => GetKeyName("Version");
        
        internal string ConsoleMinimumLevel => GetKeyName("Serilog:Console:MinimumLevel");
        internal string SerilogMinimumLevelOverrideKey => GetKeyName("Serilog:MinimumLevel:Override:");
        
        internal string BetterStackSourceToken => GetKeyName("BetterStack:SourceToken");
        internal string BetterStackEndpoint => GetKeyName("BetterStack:Endpoint");
        internal string BetterStackMinimumLevel => GetKeyName("BetterStack:MinimumLevel");
        
        internal string MicrosoftTeamsWebhookUrl => GetKeyName("MicrosoftTeams:WebhookUrl");
        internal string MicrosoftTeamsUseWorkflows => GetKeyName("MicrosoftTeams:UseWorkflows");
        internal string MicrosoftTeamsTitleTemplate => GetKeyName("MicrosoftTeams:TitleTemplate");
        internal string MicrosoftTeamsMinimumLevel => GetKeyName("MicrosoftTeams:MinimumLevel");

        internal string ConvertAzureFriendlyKeyName(string key) =>
            key.Replace('_', '.');

        private string GetKeyName(string key) => configuration["FUNCTIONS_WORKER_RUNTIME"] is not null
            ? key.Replace(':', '_')
            : key;
    }
}