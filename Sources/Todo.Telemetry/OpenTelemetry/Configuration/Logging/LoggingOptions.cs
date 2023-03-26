namespace Todo.Telemetry.OpenTelemetry.Configuration.Logging
{
    public class LoggingOptions
    {
        public bool AttachLogsToActivity { get; set; }

        public bool Enabled { get; set; }

        public bool IncludeScopes { get; set; }

        public bool IncludeFormattedMessage { get; set; }

        public bool ParseStateValues { get; set; }
    }
}
