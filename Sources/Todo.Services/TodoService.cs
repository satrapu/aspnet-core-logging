using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Todo.Persistence;
using Todo.Persistence.Entities;

namespace Todo.Services
{
    /// <summary>
    /// An <see cref="ITodoService"/> implementation which persists <see cref="TodoItem"/> instances
    /// using Entity Framework Core.
    /// </summary>
    public class TodoService : ITodoService
    {
        private readonly TodoDbContext todoDbContext;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of the <see cref="TodoService"/> class.
        /// </summary>
        /// <param name="todoDbContext">Provides access to the underlying database storing
        /// <see cref="TodoItem"/> instances.</param>
        /// <param name="logger">Provides logging services.</param>
        public TodoService(TodoDbContext todoDbContext, ILogger<TodoService> logger)
        {
            this.todoDbContext = todoDbContext ?? throw new ArgumentNullException(nameof(todoDbContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IList<TodoItemInfo> GetByQuery(TodoItemQuery todoItemQuery)
        {
            Validator.ValidateObject(todoItemQuery, new ValidationContext(todoItemQuery), true);

            IQueryable<TodoItem> matchingTodoItems =
                todoDbContext.TodoItems.Where(todoItem => todoItem.CreatedBy == todoItemQuery.User.GetUserId());

            if (todoItemQuery.Id.HasValue)
            {
                matchingTodoItems = matchingTodoItems.Where(todoItem => todoItem.Id == todoItemQuery.Id.Value);
            }

            if (!string.IsNullOrWhiteSpace(todoItemQuery.NamePattern))
            {
                matchingTodoItems = matchingTodoItems.Where(todoItem =>
                    EF.Functions.Like(todoItem.Name, todoItemQuery.NamePattern));
            }

            if (todoItemQuery.IsComplete.HasValue)
            {
                matchingTodoItems = matchingTodoItems.Where(todoItem =>
                    todoItem.IsComplete == todoItemQuery.IsComplete.Value);
            }

            IList<TodoItemInfo> result = matchingTodoItems.Select(todoItem =>
                    new TodoItemInfo
                    {
                        Id = todoItem.Id,
                        IsComplete = todoItem.IsComplete,
                        Name = todoItem.Name,
                        CreatedBy = todoItem.CreatedBy,
                        CreatedOn = todoItem.CreatedOn,
                        LastUpdatedBy = todoItem.LastUpdatedBy,
                        LastUpdatedOn = todoItem.LastUpdatedOn
                    })
                .Skip(todoItemQuery.PageIndex)
                .Take(todoItemQuery.PageSize)
                .ToList();

            return result;
        }

        public long Add(NewTodoItemInfo newTodoItemInfo)
        {
            Validator.ValidateObject(newTodoItemInfo, new ValidationContext(newTodoItemInfo), true);

            var todoItem = new TodoItem(newTodoItemInfo.Name, newTodoItemInfo.User.GetUserId())
            {
                IsComplete = newTodoItemInfo.IsComplete
            };

            todoDbContext.TodoItems.Add(todoItem);
            todoDbContext.SaveChanges();

            logger.LogInformation("Item with id {TodoItemId} has been added by user {UserId}"
                , todoItem.Id, todoItem.CreatedBy);

            return todoItem.Id;
        }

        public void Update(UpdateTodoItemInfo updateTodoItemInfo)
        {
            Validator.ValidateObject(updateTodoItemInfo, new ValidationContext(updateTodoItemInfo), true);

            TodoItem todoItem = todoDbContext.TodoItems.SingleOrDefault(myTodoItem =>
                myTodoItem.Id == updateTodoItemInfo.Id &&
                myTodoItem.CreatedBy == updateTodoItemInfo.User.GetUserId());

            if (todoItem == null)
            {
                throw new ArgumentException($"Could not find {nameof(TodoItem)} by id {updateTodoItemInfo.Id}"
                    , nameof(updateTodoItemInfo));
            }

            todoItem.IsComplete = updateTodoItemInfo.IsComplete;
            todoItem.Name = updateTodoItemInfo.Name;
            todoItem.LastUpdatedBy = updateTodoItemInfo.User.GetUserId();
            todoItem.LastUpdatedOn = DateTime.UtcNow;

            todoDbContext.TodoItems.Update(todoItem);
            todoDbContext.SaveChanges();

            logger.LogInformation("Item with id {TodoItemId} has been updated by user {UserId}"
                , todoItem.Id, todoItem.LastUpdatedBy);
        }

        public void Delete(DeleteTodoItemInfo deleteTodoItemInfo)
        {
            Validator.ValidateObject(deleteTodoItemInfo, new ValidationContext(deleteTodoItemInfo), true);

            TodoItem todoItem =
                todoDbContext.TodoItems.SingleOrDefault(myTodoItem =>
                    myTodoItem.Id == deleteTodoItemInfo.Id &&
                    myTodoItem.CreatedBy == deleteTodoItemInfo.User.GetUserId());

            if (todoItem == null)
            {
                throw new ArgumentException($"Could not find {nameof(TodoItem)} by id {deleteTodoItemInfo.Id}"
                    , nameof(deleteTodoItemInfo));
            }

            todoDbContext.TodoItems.Remove(todoItem);
            todoDbContext.SaveChanges();

            logger.LogInformation("Item with id {TodoItemId} has been deleted by user {UserId}"
                , deleteTodoItemInfo.Id, deleteTodoItemInfo.User.GetUserId());
        }
    }
}