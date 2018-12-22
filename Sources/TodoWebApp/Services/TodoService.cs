using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TodoWebApp.Models;

namespace TodoWebApp.Services
{
    /// <summary>
    /// An <see cref="ITodoService"/> implementation which persists <see cref="TodoItem"/> instances using Entity Framework Core.
    /// </summary>
    public class TodoService : ITodoService
    {
        private readonly TodoDbContext todoDbContext;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of the <see cref="TodoService"/> class.
        /// </summary>
        /// <param name="todoDbContext">Provides access to the underlying database storing <see cref="TodoItem"/> instances.</param>
        /// <param name="logger">Provides logging services.</param>
        public TodoService(TodoDbContext todoDbContext, ILogger<TodoService> logger)
        {
            this.todoDbContext = todoDbContext ?? throw new ArgumentNullException(nameof(todoDbContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IList<TodoItem> GetAll()
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("GetAll() - BEGIN");
            }

            var result = todoDbContext.TodoItems.ToList();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("GetAll() - END");
            }

            return result;
        }

        public TodoItem GetById(long id)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"GetById(long id={id}) - BEGIN");
            }

            var result = todoDbContext.TodoItems.SingleOrDefault(x => x.Id == id);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"GetById(long id={id}) - END");
            }

            return result;
        }

        public void Add(TodoItem todoItem)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Add(TodoItem todoItem={todoItem}) - BEGIN");
            }

            todoDbContext.TodoItems.Add(todoItem);
            todoDbContext.SaveChanges();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Add(TodoItem todoItem={todoItem}) - END");
            }
        }

        public void Update(TodoItem todoItem)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Update(TodoItem todoItem={todoItem}) - BEGIN");
            }

            todoDbContext.TodoItems.Update(todoItem);
            todoDbContext.SaveChanges();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Update(TodoItem todoItem={todoItem}) - END");
            }
        }

        public void Delete(TodoItem todoItem)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Delete(TodoItem todoItem={todoItem}) - BEGIN");
            }

            todoDbContext.TodoItems.Remove(todoItem);
            todoDbContext.SaveChanges();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Delete(TodoItem todoItem={todoItem}) - END");
            }
        }
    }
}
