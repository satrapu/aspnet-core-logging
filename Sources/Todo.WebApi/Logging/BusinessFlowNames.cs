namespace Todo.WebApi.Logging
{
    /// <summary>
    /// Contains the name of the business flows implemented by this application.
    /// </summary>
    public static class BusinessFlowNames
    {
        public const string ScopeKey = "BusinessFlowName";
        
        /// <summary>
        /// Contains the business flow names related to authentication.
        /// </summary>
        public static class Authentication
        {
            private const string Prefix = nameof(Authentication);
            public static readonly string GenerateJwtToken = $"{Prefix}/{nameof(GenerateJwtToken)}";
        }

        /// <summary>
        /// Contains the business flow names related to persisting todo items.
        /// </summary>
        public static class Crud
        {
            private const string Prefix = nameof(Crud);

            public static readonly string CreateTodoItem = $"{Prefix}/{nameof(CreateTodoItem)}";
            
            public static readonly string UpdateTodoItem = $"{Prefix}/{nameof(UpdateTodoItem)}";
            
            public static readonly string DeleteTodoItem = $"{Prefix}/{nameof(DeleteTodoItem)}";
            
            public static readonly string GetTodoItem = $"{Prefix}/{nameof(GetTodoItem)}";
            
            public static readonly string GetTodoItems = $"{Prefix}/{nameof(GetTodoItems)}";
        }
    }
}