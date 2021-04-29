namespace Todo.ApplicationFlows.TodoItems
{
    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Application flow used for updating an existing todo item.
    /// </summary>
    public interface IUpdateTodoItemFlow : IApplicationFlow<UpdateTodoItemInfo, object>
    {
    }
}
