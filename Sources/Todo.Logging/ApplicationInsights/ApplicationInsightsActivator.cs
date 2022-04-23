namespace Todo.Logging.ApplicationInsights
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Todo.Logging.ApplicationInsights.Configuration;

    /// <summary>
    /// Contains extension methods used for integration Azure Application Insights with this application.
    /// </summary>
    public static class ApplicationInsightsActivator
    {
        /// <summary>
        /// Adds Azure Application Insights to the current <paramref name="services"/> instance.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The given <paramref name="services"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="services"/>
        /// or <paramref name="configuration"/> is null.</exception>
        public static IServiceCollection AddApplicationInsights(this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            // ReSharper disable once SettingNotFoundInConfiguration
            IEnumerable<KeyValuePair<string, string>> configuredSerilogSinks =
                configuration.GetSection("Serilog:Using").AsEnumerable();

            bool isSerilogApplicationInsightsSinkConfigured =
                configuredSerilogSinks.Any(sink => "Serilog.Sinks.ApplicationInsights".Equals(sink.Value));

            if (isSerilogApplicationInsightsSinkConfigured)
            {
                var applicationInsightsOptions = new ApplicationInsightsOptions();
                configuration.Bind(applicationInsightsOptions);

                services.AddApplicationInsightsTelemetry(options =>
                {
                    options.InstrumentationKey = applicationInsightsOptions.InstrumentationKey;
                });
            }

            return services;
        }
    }
}
