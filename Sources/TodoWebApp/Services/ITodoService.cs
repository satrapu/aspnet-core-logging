using System.Collections.Generic;
using TodoWebApp.Models;

namespace TodoWebApp.Services
{
    /// <summary>
    /// Manages <see cref="TodoItem"/> instances.
    /// </summary>
    public interface ITodoService
    {
        /// <summary>
        /// Gets all <see cref="TodoItem"/> persisted inside the underlying database.
        /// </summary>
        /// <returns></returns>
        IList<TodoItem> GetAll();

        /// <summary>
        /// Gets a single <see cref="TodoItem"/> whose identifier matches the given <paramref name="id"/> value.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TodoItem GetById(long id);

        /// <summary>
        /// Persists a new <see cref="TodoItem"/> instance.
        /// </summary>
        /// <param name="todoItem"></param>
        void Add(TodoItem todoItem);

        /// <summary>
        /// Updates an existing <see cref="TodoItem"/> instance.
        /// </summary>
        /// <param name="todoItem"></param>
        void Update(TodoItem todoItem);

        /// <summary>
        /// Removes an existing <see cref="TodoItem"/> instance from the underlying database.
        /// </summary>
        /// <param name="todoItem"></param>
        void Delete(TodoItem todoItem);
    }
}
