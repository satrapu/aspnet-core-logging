namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains the name of the business flows implemented by this application.
    /// </summary>
    public static class BusinessFlowNames
    {
        public const string ScopeKey = "BusinessFlowName";

        /// <summary>
        /// Contains the business flow names related to authentication & authorization.
        /// </summary>
        public static class Security
        {
            private const string Prefix = nameof(Security);

            /// <summary>
            /// Business flow used for generating a JSON web token to be used for authorization purposes.
            /// </summary>
            public static readonly string GenerateJwtToken = $"{Prefix}/{nameof(GenerateJwtToken)}";
        }

        /// <summary>
        /// Contains the business flow names related to persisting todo items.
        /// </summary>
        public static class Crud
        {
            private const string Prefix = nameof(Crud);

            /// <summary>
            /// Business flow used for creating a new todo item.
            /// </summary>
            public static readonly string CreateTodoItem = $"{Prefix}/{nameof(CreateTodoItem)}";

            /// <summary>
            /// Business flow used for updating an existing todo item.
            /// </summary>
            public static readonly string UpdateTodoItem = $"{Prefix}/{nameof(UpdateTodoItem)}";

            /// <summary>
            /// Business flow used for removing an existing todo item.
            /// </summary>
            public static readonly string DeleteTodoItem = $"{Prefix}/{nameof(DeleteTodoItem)}";

            /// <summary>
            /// Business flow used for fetching the details of an existing todo item.
            /// </summary>
            public static readonly string GetTodoItem = $"{Prefix}/{nameof(GetTodoItem)}";

            /// <summary>
            /// Business flow used for fetching the details of a list of existing todo items.
            /// </summary>
            public static readonly string GetTodoItems = $"{Prefix}/{nameof(GetTodoItems)}";
        }
    }
}