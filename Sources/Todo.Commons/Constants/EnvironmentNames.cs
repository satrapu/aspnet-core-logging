namespace Todo.Commons.Constants
{
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Contains constants related to the environments where this application will run.
    /// </summary>
    public static class EnvironmentNames
    {
        /// <summary>
        /// Represents the name of the local development environment.
        /// </summary>
        public static readonly string Development = Environments.Development;

        /// <summary>
        /// Represents the name of the environment where integration tests are run
        /// (usually a local development machine or a CI environment, like Azure DevOps).
        /// </summary>
        public const string IntegrationTests = "IntegrationTests";

        /// <summary>
        /// Represents the name of the Azure environment used for demonstrating various application features.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public const string DemoInAzure = "DemoInAzure";

        /// <summary>
        /// Represents the name of the staging environment.
        /// </summary>
        public static readonly string Staging = Environments.Staging;

        /// <summary>
        /// Represents the name of the production environment.
        /// </summary>
        public static readonly string Production = Environments.Production;
    }
}
