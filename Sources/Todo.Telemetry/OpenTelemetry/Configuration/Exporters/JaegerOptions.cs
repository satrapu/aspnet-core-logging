namespace Todo.Telemetry.OpenTelemetry.Configuration.Exporters
{
    public class JaegerOptions
    {
        public string AgentHost { get; set; }

        public int AgentPort { get; set; }

        public bool Enabled { get; set; }
    }
}
