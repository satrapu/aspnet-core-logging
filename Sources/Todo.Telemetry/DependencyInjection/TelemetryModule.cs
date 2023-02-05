namespace Todo.Telemetry.DependencyInjection
{
    using Autofac;

    using Commons.ApplicationEvents;

    using Http;

    using Serilog;

    /// <summary>
    /// Configures telemetry related services used by this application.
    /// </summary>
    public class TelemetryModule : Module
    {
        /// <summary>
        /// Gets or sets whether HTTP requests and their responses will be logged.
        /// </summary>
        public bool EnableHttpLogging { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            if (EnableHttpLogging)
            {
                builder
                    .RegisterType<HttpLoggingService>()
                    .AsSelf()
                    .As<IHttpObjectConverter>()
                    .As<IHttpContextLoggingHandler>()
                    .SingleInstance();
            }

            builder
                .RegisterType<FileSinkMetadataLogger>()
                .As<IApplicationStartedEventListener>()
                .SingleInstance();
        }
    }
}
