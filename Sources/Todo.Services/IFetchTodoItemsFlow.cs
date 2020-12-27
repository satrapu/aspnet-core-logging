using System.Collections.Generic;

namespace Todo.Services
{
    /// <summary>
    /// Application flow used for fetching <see cref="TodoItemInfo"/> instances matching a given query.
    /// </summary>
    public interface IFetchTodoItemsFlow : IApplicationFlow<TodoItemQuery, IList<TodoItemInfo>>
    {
    }
}