using System.Collections.Generic;
using System.Threading.Tasks;

namespace Todo.Services
{
    /// <summary>
    /// Manages the lifecycle of todo items.
    /// </summary>
    public interface ITodoService
    {
        /// <summary>
        /// Fetches all todo items found inside the underlying persistent storage which match a given set of conditions.
        /// </summary>
        /// <param name="todoItemQuery">The conditions to use when matching todo items.</param>
        /// <returns>A list of todo items matching the given conditions.</returns>
        Task<IList<TodoItemInfo>> GetByQueryAsync(TodoItemQuery todoItemQuery);

        /// <summary>
        /// Persists a new todo item to the underlying persistent storage.
        /// </summary>
        /// <param name="newTodoItemInfo">The template used for creating the new todo item.</param>
        /// <returns>The identified of the new todo item.</returns>
        Task<long> AddAsync(NewTodoItemInfo newTodoItemInfo);

        /// <summary>
        /// Updates an existing todo item found inside the underlying persistent storage.
        /// </summary>
        /// <param name="updateTodoItemInfo">The template used for updating the existing todo item.</param>
        Task UpdateAsync(UpdateTodoItemInfo updateTodoItemInfo);

        /// <summary>
        /// Removes an existing todo item from the underlying persistent storage.
        /// </summary>
        /// <param name="deleteTodoItemInfo">The template used for removing the existing todo item.</param>
        Task DeleteAsync(DeleteTodoItemInfo deleteTodoItemInfo);
    }
}