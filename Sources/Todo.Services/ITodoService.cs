using System.Collections.Generic;
using System.Threading.Tasks;
using Todo.Persistence.Entities;

namespace Todo.Services
{
    /// <summary>
    /// Manages <see cref="TodoItem"/> instances.
    /// </summary>
    public interface ITodoService
    {
        /// <summary>
        /// Gets all <see cref="TodoItem"/> instances persisted inside the underlying database
        /// which match the given <paramref name="todoItemQuery"/>.
        /// </summary>
        /// <param name="todoItemQuery"></param>
        /// <returns></returns>
        Task<IList<TodoItemInfo>> GetByQueryAsync(TodoItemQuery todoItemQuery);

        /// <summary>
        /// Persists a new <see cref="TodoItem"/> instance.
        /// </summary>
        /// <param name="newTodoItemInfo"></param>
        /// <returns></returns>
        Task<long> AddAsync(NewTodoItemInfo newTodoItemInfo);

        /// <summary>
        /// Updates an existing <see cref="TodoItem"/> instance.
        /// </summary>
        /// <param name="updateTodoItemInfo"></param>
        Task UpdateAsync(UpdateTodoItemInfo updateTodoItemInfo);

        /// <summary>
        /// Removes an existing <see cref="TodoItem"/> instance from the underlying database.
        /// </summary>
        /// <param name="deleteTodoItemInfo"></param>
        Task DeleteAsync(DeleteTodoItemInfo deleteTodoItemInfo);
    }
}
