using Todo.Services.TodoItemLifecycleManagement;

namespace Todo.ApplicationFlows.TodoItems
{
    /// <summary>
    /// Application flow used for creating a new todo item.
    /// </summary>
    public interface IAddTodoItemFlow : IApplicationFlow<NewTodoItemInfo, long>
    {
    }
}