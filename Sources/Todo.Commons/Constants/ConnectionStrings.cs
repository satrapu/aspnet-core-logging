namespace Todo.Commons.Constants
{
    /// <summary>
    /// Contains constants related to the connection strings used by this application.
    /// </summary>
    public static class ConnectionStrings
    {
        /// <summary>
        /// Represents the name of the connection string to be used when running integration tests.
        /// </summary>
        public const string UsedByIntegrationTests = "TodoForIntegrationTests";

        /// <summary>
        /// Represents the name of the connection string to be used when running acceptance tests.
        /// </summary>
        public const string UsedByAcceptanceTests = "TodoForAcceptanceTests";

        /// <summary>
        /// Represents the name of the connection string to be used when running the application.
        /// </summary>
        public const string UsedByApplication = "Todo";
    }
}
