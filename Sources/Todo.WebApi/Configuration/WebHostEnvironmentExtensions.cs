namespace Todo.WebApi.Configuration
{
    using System;

    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// Contains extension methods applicable to <see cref="IWebHostEnvironment"/> instances.
    /// </summary>
    public static class WebHostEnvironmentExtensions
    {
        private const string IntegrationTestsEnvironmentName = "IntegrationTests";

        /// <summary>
        /// Checks if the current host environment name is <see cref="IntegrationTestsEnvironmentName"/>.
        /// </summary>
        /// <param name="webHostEnvironment">The <see cref="IWebHostEnvironment"/> instance to check.</param>
        /// <returns>True if the environment name is <see cref="IntegrationTestsEnvironmentName"/>,
        /// otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the given <paramref name="webHostEnvironment"/>
        /// is null.</exception>
        public static bool IsIntegrationTests(this IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment is null)
            {
                throw new ArgumentNullException(nameof(webHostEnvironment));
            }

            return IntegrationTestsEnvironmentName.Equals(webHostEnvironment.EnvironmentName);
        }
    }
}
