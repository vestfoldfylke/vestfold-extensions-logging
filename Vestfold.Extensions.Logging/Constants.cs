namespace Vestfold.Extensions.Logging;

internal static class Constants
{
    internal static class Properties
    {
        internal const string AppName = "AppName";
        internal const string Version = "Version";
    }
    
    internal static class ConfigurationKeys
    {
        internal static string AppName => "AppName";
        internal static string Version => "AppVersion";
        
        internal static string ConsoleMinimumLevel => "Serilog:Console:MinimumLevel";
        internal static string SerilogMinimumLevelOverrideKey => "Serilog:MinimumLevel:Override:";
        
        internal static string BetterStackSourceToken => "BetterStack:SourceToken";
        internal static string BetterStackEndpoint => "BetterStack:Endpoint";
        internal static string BetterStackMinimumLevel => "BetterStack:MinimumLevel";
        
        internal static string MicrosoftTeamsWebhookUrl => "MicrosoftTeams:WebhookUrl";
        internal static string MicrosoftTeamsUseWorkflows => "MicrosoftTeams:UseWorkflows";
        internal static string MicrosoftTeamsTitleTemplate => "MicrosoftTeams:TitleTemplate";
        internal static string MicrosoftTeamsMinimumLevel => "MicrosoftTeams:MinimumLevel";
        
        internal static string FilePath => "Serilog:File:Path";
        internal static string FileMinimumLevel => "Serilog:File:MinimumLevel";
        internal static string FileRollingInterval => "Serilog:File:RollingInterval";

        internal static string ConvertAzureFriendlyKeyName(string key) =>
            key.Replace('_', '.');
    }
}