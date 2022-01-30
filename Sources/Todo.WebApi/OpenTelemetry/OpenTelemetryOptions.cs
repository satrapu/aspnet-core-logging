namespace Todo.WebApi.OpenTelemetry
{
    using Todo.WebApi.OpenTelemetry.Exporters;
    using Todo.WebApi.OpenTelemetry.Instrumentation;

    public class OpenTelemetryOptions
    {
        public OpenTelemetryInstrumentationOptions Instrumentation { get; set; }
        public OpenTelemetryExporterOptions Exporters { get; set; }
    }
}
