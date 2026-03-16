namespace Vestfold.Extensions.Logging.Models;

internal interface ISerilogSinkConfiguration
{
    /**
     * Does this sink have all the required configuration values to be enabled?
     */
    bool Enabled { get; }
    
    /**
     * Log events to exclude from being sent to this logger (identified by the presence of these properties)<br /><br />
     *
     * If set to an empty list, all log events will be sent to this logger
     */
    string[] PropertiesToExclude { get; }
    
    /**
     * Log events to include for this logger (identified by the presence of these properties)<br /><br />
     *
     * If set to an empty list, all log events will be sent to this logger
     */
    string[] PropertiesToInclude { get; }
}