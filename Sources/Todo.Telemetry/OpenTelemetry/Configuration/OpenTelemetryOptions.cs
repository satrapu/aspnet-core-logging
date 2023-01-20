namespace Todo.Telemetry.OpenTelemetry.Configuration
{
    using Exporters;
    using Instrumentation;

    public class OpenTelemetryOptions
    {
        public bool AttachLogsToActivity { get; set; }

        public bool Enabled { get; set; }

        public bool IncludeScopes { get; set; }

        public bool IncludeFormattedMessage { get; set; }

        public bool ParseStateValues { get; set; }

        public OpenTelemetryInstrumentationOptions Instrumentation { get; set; }

        public OpenTelemetryExporterOptions Exporters { get; set; }
    }
}
