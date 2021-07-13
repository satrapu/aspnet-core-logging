namespace Todo.Commons
{
    /// <summary>
    /// Contains constants used by the various application components.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Contains constants used for logging.
        /// </summary>
        public static class Logging
        {
            /// <summary>
            /// Represents the name of the folder where log files will be created.
            /// </summary>
            public const string LogsHomeEnvironmentVariable = "LOGS_HOME";

            /// <summary>
            /// Represents the identifier of the conversation grouping several events related to the same operation.
            /// </summary>
            public const string ConversationId = "ConversationId";

            /// <summary>
            /// Represents the name of an application flow.
            /// </summary>
            public const string ApplicationFlowName = "ApplicationFlowName";
        }

        /// <summary>
        /// Contains the names of the environments where this application will run.
        /// </summary>
        public static class EnvironmentNames
        {
            /// <summary>
            /// Represents the local development environment.
            /// </summary>
            public const string Development = "Development";

            /// <summary>
            /// Represents an Azure environment used for demonstrating various application features.
            /// </summary>
            // ReSharper disable once UnusedMember.Global
            public const string DemoInAzure = "DemoInAzure";

            /// <summary>
            /// Represents the environment where integration tests are run (usually a local development machine
            /// or a CI environment, like Azure DevOps).
            /// </summary>
            public const string IntegrationTests = "IntegrationTests";
        }
    }
}
