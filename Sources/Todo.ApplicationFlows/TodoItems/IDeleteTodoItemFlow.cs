namespace Todo.ApplicationFlows.TodoItems
{
    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Application flow used for deleting an existing todo item.
    /// </summary>
    public interface IDeleteTodoItemFlow : IApplicationFlow<DeleteTodoItemInfo, object>
    {
    }
}
