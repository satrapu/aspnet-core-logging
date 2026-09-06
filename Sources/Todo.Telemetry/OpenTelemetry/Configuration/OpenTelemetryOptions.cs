namespace Todo.Telemetry.OpenTelemetry.Configuration
{
    using Exporters;

    using Logging;

    public class OpenTelemetryOptions
    {
        public bool Enabled { get; set; }

        public LoggingOptions Logging { get; set; }

        public OpenTelemetryExporterOptions Exporters { get; set; }
    }
}
