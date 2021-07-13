namespace Todo.ApplicationFlows.TodoItems
{
    using System.Diagnostics.CodeAnalysis;

    using Services.TodoItemManagement;

    /// <summary>
    /// Application flow used for creating a new todo item.
    /// </summary>
    [SuppressMessage("ReSharper", "S1135", Justification = "The todo word represents an entity")]
    public interface IAddTodoItemFlow : IApplicationFlow<NewTodoItemInfo, long>
    {
    }
}
