namespace Todo.ApplicationFlows.TodoItems
{
    using System.Collections.Generic;

    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// Application flow used for fetching <see cref="TodoItemInfo"/> instances matching a given query.
    /// </summary>
    public interface IFetchTodoItemsFlow : IApplicationFlow<TodoItemQuery, IList<TodoItemInfo>>
    {
    }
}
