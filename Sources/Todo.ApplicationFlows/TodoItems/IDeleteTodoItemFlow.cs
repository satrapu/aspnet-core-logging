namespace Todo.ApplicationFlows.TodoItems
{
    using System.Diagnostics.CodeAnalysis;

    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Application flow used for deleting an existing todo item.
    /// </summary>
    [SuppressMessage("ReSharper", "S1135", Justification = "The todo word represents an entity")]
    public interface IDeleteTodoItemFlow : IApplicationFlow<DeleteTodoItemInfo, object>
    {
    }
}
