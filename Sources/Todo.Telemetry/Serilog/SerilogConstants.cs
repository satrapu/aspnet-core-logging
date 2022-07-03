namespace Todo.Telemetry.Serilog
{
    /// <summary>
    /// Contains constants related to Serilog configuration.
    /// </summary>
    internal static class SerilogConstants
    {
        /// <summary>
        /// Contains constants related to Serilog configuration sections.
        /// </summary>
        internal static class SectionNames
        {
            internal const string Using = "Serilog:Using";
        }

        /// <summary>
        /// Contains constants representing the Serilog sinks used by this application.
        /// </summary>
        internal static class SinkShortNames
        {
            internal const string ApplicationInsights = "Serilog.Sinks.ApplicationInsights";
            internal const string File = "Serilog.Sinks.File";
        }
    }
}
