namespace Todo.Telemetry.OpenTelemetry.Configuration.Exporters
{
    public class OpenTelemetryExporterOptions
    {
        public AzureMonitorOptions AzureMonitor { get; set; }
        public JaegerOptions Jaeger { get; set; }
    }
}
