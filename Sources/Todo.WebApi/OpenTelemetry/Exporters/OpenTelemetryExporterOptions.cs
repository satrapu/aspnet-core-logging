namespace Todo.WebApi.OpenTelemetry.Exporters
{
    public class OpenTelemetryExporterOptions
    {
        public JaegerOptions Jaeger { get; set; }
        public OtplOptions Otpl { get; set; }
    }
}
