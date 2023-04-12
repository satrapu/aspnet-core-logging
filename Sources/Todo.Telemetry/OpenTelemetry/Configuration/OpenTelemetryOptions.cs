namespace Todo.Telemetry.OpenTelemetry.Configuration
{
    using Exporters;

    using Instrumentation;

    using Logging;

    public class OpenTelemetryOptions
    {
        public bool Enabled { get; set; }

        public LoggingOptions Logging { get; set; }

        public OpenTelemetryInstrumentationOptions Instrumentation { get; set; }

        public OpenTelemetryExporterOptions Exporters { get; set; }
    }
}
