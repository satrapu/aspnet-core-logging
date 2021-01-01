using Todo.Services.TodoItemLifecycleManagement;

namespace Todo.ApplicationFlows.TodoItems
{
    /// <summary>
    /// Application flow used for fetching a <see cref="TodoItemInfo"/> instance matching a given identifier.
    /// </summary>
    public interface IFetchTodoItemByIdFlow : IApplicationFlow<long, TodoItemInfo>
    {
        // TODO: Bogdan Marian 2021-01-01: Use a model decorated with validation related attributes instead
        // of a raw long value.
    }
}