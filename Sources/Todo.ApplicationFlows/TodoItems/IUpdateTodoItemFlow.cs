using Todo.Services.TodoItemLifecycleManagement;

namespace Todo.ApplicationFlows.TodoItems
{
    /// <summary>
    /// Application flow used for updating an existing todo item.
    /// </summary>
    public interface IUpdateTodoItemFlow : IApplicationFlow<UpdateTodoItemInfo, object>
    {
    }
}