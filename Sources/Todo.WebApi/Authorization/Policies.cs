namespace Todo.WebApi.Authorization
{
    public static class Policies
    {
        public static class TodoItems
        {
            public const string CreateTodoItem = "create:todo";
            public const string UpdateTodoItem = "update:todo";
            public const string DeleteTodoItem = "delete:todo";
            public const string GetTodoItems = "get:todo";
        }
    }
}