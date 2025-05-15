using Microsoft.Extensions.Configuration;

namespace VFK.Extensions.Logging;

public static class Constants
{
    public static class Properties
    {
        public const string AppName = "AppName";
        public const string Version = "Version";
    }
    
    public class ConfigurationKeys(IConfiguration configuration)
    {
        public string AppName => GetValue("AppName");
        public string SerilogMinimumLevelOverrideKey => GetValue("Serilog:MinimumLevel:Override:");
        public string BetterStackSourceToken => GetValue("BetterStack:SourceToken");
        public string BetterStackEndpoint => GetValue("BetterStack:Endpoint");
        public string BetterStackMinimumLevel => GetValue("BetterStack:MinimumLevel");
        public string ConsoleMinimumLevel => GetValue("Serilog:Console:MinimumLevel");
        public string Version => GetValue("Version");

        private string GetValue(string key) => configuration["AZURE_FUNCTIONS_ENVIRONMENT"] is not null
            ? key.Replace(':', '_')
            : key;
    }
}