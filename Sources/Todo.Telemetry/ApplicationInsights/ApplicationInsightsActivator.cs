namespace Todo.Telemetry.ApplicationInsights
{
    using System;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Todo.Telemetry.ApplicationInsights.Configuration;
    using Todo.Telemetry.Serilog;

    /// <summary>
    /// Contains extension methods used for integrating Azure Application Insights with this application.
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

            if (SerilogActivator.IsApplicationInsightsSinkConfigured(configuration))
            {
                var applicationInsightsOptions = new ApplicationInsightsOptions();
                configuration.Bind(applicationInsightsOptions);

                services.AddApplicationInsightsTelemetry(options => options.InstrumentationKey = applicationInsightsOptions.InstrumentationKey);
            }

            return services;
        }
    }
}
