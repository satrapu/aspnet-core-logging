namespace Todo.Telemetry
{
    internal static class SerilogConstants
    {
        internal static class SectionNames
        {
            internal const string Using = "Serilog:Using";
        }

        internal static class SinkShortNames
        {
            internal const string ApplicationInsights = "Serilog.Sinks.ApplicationInsights";
            internal const string File = "Serilog.Sinks.File";
        }
    }
}
