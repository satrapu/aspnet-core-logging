using System.Collections.Generic;
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
        IList<TodoItemInfo> GetByQuery(TodoItemQuery todoItemQuery);

        /// <summary>
        /// Persists a new <see cref="TodoItem"/> instance.
        /// </summary>
        /// <param name="newTodoItemInfo"></param>
        /// <returns></returns>
        long Add(NewTodoItemInfo newTodoItemInfo);

        /// <summary>
        /// Updates an existing <see cref="TodoItem"/> instance.
        /// </summary>
        /// <param name="updateTodoItemInfo"></param>
        void Update(UpdateTodoItemInfo updateTodoItemInfo);

        /// <summary>
        /// Removes an existing <see cref="TodoItem"/> instance from the underlying database.
        /// </summary>
        /// <param name="deleteTodoItemInfo"></param>
        void Delete(DeleteTodoItemInfo deleteTodoItemInfo);
    }
}
