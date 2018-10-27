using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TodoWebApp.Models;

namespace TodoWebApp.Services
{
    /// <summary>
    /// An <see cref="ITodoService"/> implementation which persists <see cref="TodoItem"/> using Entity Framework Core.
    /// </summary>
    public class TodoService : ITodoService
    {
        private readonly TodoDbContext todoDbContext;
        private readonly ILogger logger;

        public TodoService(TodoDbContext todoDbContext, ILogger<TodoService> logger)
        {
            this.todoDbContext = todoDbContext ?? throw new ArgumentNullException(nameof(todoDbContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IList<TodoItem> GetAll()
        {
            return todoDbContext.TodoItems.ToList();
        }

        public TodoItem GetById(long id)
        {
            return todoDbContext.TodoItems.SingleOrDefault(x => x.Id == id);
        }

        public void Add(TodoItem todoItem)
        {
            todoDbContext.TodoItems.Add(todoItem);
            todoDbContext.SaveChanges();
        }

        public void Update(TodoItem todoItem)
        {
            todoDbContext.TodoItems.Update(todoItem);
            todoDbContext.SaveChanges();
        }

        public void Delete(TodoItem todoItem)
        {
            todoDbContext.TodoItems.Remove(todoItem);
            todoDbContext.SaveChanges();
        }
    }
}
