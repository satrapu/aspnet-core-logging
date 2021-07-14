namespace Todo.DependencyInjection
{
    using Autofac;

    using Logging.Http;

    /// <summary>
    /// Configures logging related services used by this application.
    /// </summary>
    public class LoggingModule : Module
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
        }
    }
}
