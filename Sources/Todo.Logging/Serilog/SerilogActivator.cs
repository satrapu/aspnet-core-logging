namespace Todo.Logging.Serilog
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Commons;

    using global::Serilog;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Contains extension methods used for integration Serilog with this application.
    /// </summary>
    public static class SerilogActivator
    {
        /// <summary>
        /// Adds Serilog logging provider to the current <paramref name="services"/> instance.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The given <paramref name="services"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Throw when either <paramref name="services"/>
        /// or <paramref name="configuration"/> is null.</exception>
        public static IServiceCollection ActivateSerilog(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddLogging(loggingBuilder =>
            {
                ConfigureFileSink(configuration);

                loggingBuilder
                    .ClearProviders()
                    .AddSerilog(new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger(), dispose: true);
            });

            return services;
        }

        private static void ConfigureFileSink(IConfiguration configuration)
        {
            // ReSharper disable once SettingNotFoundInConfiguration
            IEnumerable<KeyValuePair<string, string>> configuredSerilogSinks =
                configuration.GetSection("Serilog:Using").AsEnumerable().ToList();

            bool isSerilogFileSinkConfigured =
                configuredSerilogSinks.Any(sink => "Serilog.Sinks.File".Equals(sink.Value));

            if (!isSerilogFileSinkConfigured)
            {
                return;
            }

            string logsHomeEnvironmentVariableName = Constants.Logging.LogsHomeEnvironmentVariable;
            string logsHomeDirectoryPath = Environment.GetEnvironmentVariable(logsHomeEnvironmentVariableName);

            if (string.IsNullOrWhiteSpace(logsHomeDirectoryPath) || !Directory.Exists(logsHomeDirectoryPath))
            {
                var currentWorkingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
                DirectoryInfo logsHomeDirectory = currentWorkingDirectory.CreateSubdirectory("Logs");
                Environment.SetEnvironmentVariable(logsHomeEnvironmentVariableName, logsHomeDirectory.FullName);
            }
        }
    }
}
