namespace Todo.ApplicationFlows.TodoItems
{
    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Application flow used for fetching a <see cref="TodoItemInfo"/> instance matching a given identifier.
    /// </summary>
    public interface IFetchTodoItemByIdFlow : IApplicationFlow<long, TodoItemInfo>
    {
    }
}
