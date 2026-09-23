using Serilog.Events;

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

    /**
     * If set, log events at this level or higher bypass the <see cref="PropertiesToExclude"/> filter
     * and are sent to this sink even when they carry an excluded property.<br /><br />
     *
     * This exists because <c>LogContext.PushProperty</c> is scope-based: a <c>SecurityAudit</c> scope
     * marks every event inside the block — including errors and exception logs — so a naive exclusion
     * would silence the very notifications an operator needs to see.<br /><br />
     *
     * If <c>null</c>, <see cref="PropertiesToExclude"/> is applied regardless of the event's level.
     */
    LogEventLevel? AlwaysIncludeAtOrAboveLevel { get; }
}