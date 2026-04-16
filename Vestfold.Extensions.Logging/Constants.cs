namespace Vestfold.Extensions.Logging;

public static class Constants
{
    public static class Properties
    {
        /**
         * Log events which has this property will be sent to security audit loggers (e.g. Azure Log Analytics)
         */
        public const string SecurityAudit = "SecurityAudit";
        
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

        internal static string AzureLogAnalyticsClientId => "Serilog:AzureLogAnalytics:ClientId";
        internal static string AzureLogAnalyticsClientSecret => "Serilog:AzureLogAnalytics:ClientSecret";
        internal static string AzureLogAnalyticsEndpoint => "Serilog:AzureLogAnalytics:Endpoint";
        internal static string AzureLogAnalyticsImmutableId => "Serilog:AzureLogAnalytics:ImmutableId";
        internal static string AzureLogAnalyticsStreamName => "Serilog:AzureLogAnalytics:StreamName";
        internal static string AzureLogAnalyticsTenantId => "Serilog:AzureLogAnalytics:TenantId";
        internal static string AzureLogAnalyticsMinimumLevel => "Serilog:AzureLogAnalytics:MinimumLevel";
        internal static string AzureLogAnalyticsBatchSize => "Serilog:AzureLogAnalytics:BatchSize";
        internal static string AzureLogAnalyticsBufferSize => "Serilog:AzureLogAnalytics:BufferSize";
        internal static string AzureLogAnalyticsPeriodSeconds => "Serilog:AzureLogAnalytics:PeriodSeconds";

        internal static string ConvertAzureFriendlyKeyName(string key) =>
            key.Replace('_', '.');
    }
}