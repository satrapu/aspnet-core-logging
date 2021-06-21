namespace Todo.Integrations.MiniProfiler
{
    using System;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Contains extension methods applicable to <see cref="IServiceCollection"/> class.
    /// </summary>
    public static class MiniProfilerActivator
    {
        /// <summary>
        /// Adds MiniProfiler to the current <paramref name="services"/> instance.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        /// <param name="configuration">The application configuration.</param>
        public static IServiceCollection ActivateMiniProfiler(this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
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
    }
}
