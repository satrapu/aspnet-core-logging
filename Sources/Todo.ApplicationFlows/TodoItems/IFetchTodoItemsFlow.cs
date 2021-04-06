using System.Collections.Generic;
using Todo.Services.TodoItemLifecycleManagement;

namespace Todo.ApplicationFlows.TodoItems
{
    /// <summary>
    /// Application flow used for fetching <see cref="TodoItemInfo"/> instances matching a given query.
    /// </summary>
    public interface IFetchTodoItemsFlow : IApplicationFlow<TodoItemQuery, IList<TodoItemInfo>>
    {
    }
}