namespace Todo.Integrations.MiniProfiler
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class MiniProfilerActivator
    {
        public static void ActivateMiniProfiler(this IServiceCollection services, IConfiguration configuration)
        {
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
        }
    }
}
