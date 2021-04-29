namespace Todo.ApplicationFlows.TodoItems
{
    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Application flow used for creating a new todo item.
    /// </summary>
    public interface IAddTodoItemFlow : IApplicationFlow<NewTodoItemInfo, long>
    {
    }
}
