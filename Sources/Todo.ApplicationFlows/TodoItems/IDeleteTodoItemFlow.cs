using Todo.Services.TodoItemLifecycleManagement;

namespace Todo.ApplicationFlows.TodoItems
{
    /// <summary>
    /// Application flow used for deleting an existing todo item.
    /// </summary>
    public interface IDeleteTodoItemFlow : IApplicationFlow<DeleteTodoItemInfo, object>
    {
    }
}