namespace Todo.Profiling
{
    using System;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Contains extension methods used for integration MiniProfiler with this application.
    /// </summary>
    public static class MiniProfilerActivator
    {
        /// <summary>
        /// Adds MiniProfiler to the current <paramref name="services"/> instance.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The given <paramref name="services"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="services"/>
        /// or <paramref name="configuration"/> is null.</exception>
        public static IServiceCollection ActivateMiniProfiler(this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // ReSharper disable once SettingNotFoundInConfiguration
            bool isMiniProfilerEnabled = configuration.GetValue<bool>("MiniProfiler:Enabled");

            if (!isMiniProfilerEnabled)
            {
                return services;
            }

            // Configure MiniProfiler for Web API and EF Core.
            // Based on: https://dotnetthoughts.net/using-miniprofiler-in-aspnetcore-webapi/.
            services
                .AddMemoryCache()
                .AddMiniProfiler(options =>
                {
                    // MiniProfiler URLs (assuming options.RouteBasePath has been set to '/miniprofiler')
                    // - show all requests:         /miniprofiler/results-index
                    // - show current request:      /miniprofiler/results
                    // - show all requests as JSON: /miniprofiler/results-list
                    // ReSharper disable once SettingNotFoundInConfiguration
                    options.RouteBasePath = configuration.GetValue<string>("MiniProfiler:RouteBasePath");
                    options.EnableServerTimingHeader = true;
                })
                .AddEntityFramework();

            return services;
        }

        /// <summary>
        /// Adds MiniProfiler to the current ASP.NET Core request processing pipeline.
        /// </summary>
        /// <param name="applicationBuilder">Configures ASP.NET Core request processing pipeline.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The given <paramref name="applicationBuilder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="applicationBuilder"/>
        /// or <paramref name="configuration"/> is null.</exception>
        public static IApplicationBuilder UseMiniProfiler(this IApplicationBuilder applicationBuilder,
            IConfiguration configuration)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // ReSharper disable once SettingNotFoundInConfiguration
            bool isMiniProfilerEnabled = configuration.GetValue<bool>("MiniProfiler:Enabled");

            if (!isMiniProfilerEnabled)
            {
                return applicationBuilder;
            }

            return applicationBuilder.UseMiniProfiler();
        }
    }
}
