using System.Collections.Generic;
using TodoWebApp.Models;

namespace TodoWebApp.Services
{
    /// <summary>
    /// Manages <see cref="TodoItem"/> instances.
    /// </summary>
    public interface ITodoService
    {
        IList<TodoItem> GetAll();

        TodoItem GetById(long id);

        void Add(TodoItem todoItem);

        void Update(TodoItem todoItem);

        void Delete(TodoItem todoItem);
    }
}
