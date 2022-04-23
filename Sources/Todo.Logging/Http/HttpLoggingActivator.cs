namespace Todo.Logging.Http
{
    using System;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Contains extension methods used for enabling HTTP logging for this application.
    /// </summary>
    public static class HttpLoggingActivator
    {
        /// <summary>
        /// Enables logging each HTTP request and its response inside the current ASP.NET Core request processing
        /// pipeline.
        /// </summary>
        /// <param name="applicationBuilder">Configures ASP.NET Core request processing pipeline.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The given <paramref name="applicationBuilder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="applicationBuilder"/>
        /// or <paramref name="configuration"/> is null.</exception>
        public static IApplicationBuilder UseHttpLogging(this IApplicationBuilder applicationBuilder,
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
            bool isHttpLoggingEnabled = configuration.GetValue<bool>("HttpLogging:Enabled");

            if (isHttpLoggingEnabled)
            {
                applicationBuilder.UseMiddleware<HttpLoggingMiddleware>();
            }

            return applicationBuilder;
        }
    }
}
