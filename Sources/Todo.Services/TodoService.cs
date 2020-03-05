using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
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
        private const string SortByCreatedOn = nameof(TodoItem.CreatedOn);
        private const string SortById = nameof(TodoItem.Id);
        private const string SortByLastUpdatedOn = nameof(TodoItem.LastUpdatedOn);
        private const string SortByName = nameof(TodoItem.Name);

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

        public async Task<IList<TodoItemInfo>> GetByQueryAsync(TodoItemQuery todoItemQuery)
        {
            Validator.ValidateObject(todoItemQuery, new ValidationContext(todoItemQuery), true);

            IQueryable<TodoItem> todoItems = FilterItems(todoItemQuery);
            todoItems = SortItems(todoItems, todoItemQuery);
            todoItems = PaginateItems(todoItems, todoItemQuery);
            IQueryable<TodoItemInfo> todoItemInfos = ProjectItems(todoItems);
            IList<TodoItemInfo> result = await todoItemInfos.ToListAsync().ConfigureAwait(false);

            logger.LogInformation("Fetched {TodoItemsCount} todo item(s) for user {UserId} using query {TodoItemQuery}",
                result.Count, todoItemQuery.User.GetUserId(), todoItemQuery);

            return result;
        }

        public long Add(NewTodoItemInfo newTodoItemInfo)
        {
            Validator.ValidateObject(newTodoItemInfo, new ValidationContext(newTodoItemInfo), true);

            var newTodoItem = new TodoItem(newTodoItemInfo.Name, newTodoItemInfo.User.GetUserId())
            {
                IsComplete = newTodoItemInfo.IsComplete
            };

            todoDbContext.TodoItems.Add(newTodoItem);
            todoDbContext.SaveChanges();

            logger.LogInformation("Item with id {TodoItemId} has been added by user {UserId}"
                , newTodoItem.Id, newTodoItem.CreatedBy);

            return newTodoItem.Id;
        }

        public void Update(UpdateTodoItemInfo updateTodoItemInfo)
        {
            Validator.ValidateObject(updateTodoItemInfo, new ValidationContext(updateTodoItemInfo), true);

            TodoItem existingTodoItem = GetExistingTodoItem(updateTodoItemInfo.Id, updateTodoItemInfo.User);
            existingTodoItem.IsComplete = updateTodoItemInfo.IsComplete;
            existingTodoItem.Name = updateTodoItemInfo.Name;
            existingTodoItem.LastUpdatedBy = updateTodoItemInfo.User.GetUserId();
            existingTodoItem.LastUpdatedOn = DateTime.UtcNow;

            todoDbContext.TodoItems.Update(existingTodoItem);
            todoDbContext.SaveChanges();

            logger.LogInformation("Item with id {TodoItemId} has been updated by user {UserId}"
                , existingTodoItem.Id, existingTodoItem.LastUpdatedBy);
        }

        public void Delete(DeleteTodoItemInfo deleteTodoItemInfo)
        {
            Validator.ValidateObject(deleteTodoItemInfo, new ValidationContext(deleteTodoItemInfo), true);
            TodoItem existingTodoItem = GetExistingTodoItem(deleteTodoItemInfo.Id, deleteTodoItemInfo.User);

            todoDbContext.TodoItems.Remove(existingTodoItem);
            todoDbContext.SaveChanges();

            logger.LogInformation("Item with id {TodoItemId} has been deleted by user {UserId}"
                , deleteTodoItemInfo.Id, deleteTodoItemInfo.User.GetUserId());
        }

        private TodoItem GetExistingTodoItem(long? id, ClaimsPrincipal owner)
        {
            TodoItem existingTodoItem = todoDbContext.TodoItems.SingleOrDefault(todoItem =>
                todoItem.Id == id && todoItem.CreatedBy == owner.GetUserId());

            if (existingTodoItem == null)
            {
                throw new ArgumentException($"Could not find {nameof(TodoItem)} by id {id}", nameof(id));
            }

            return existingTodoItem;
        }

        private IQueryable<TodoItem> FilterItems(TodoItemQuery todoItemQuery)
        {
            IQueryable<TodoItem> todoItems =
                todoDbContext.TodoItems.Where(todoItem => todoItem.CreatedBy == todoItemQuery.User.GetUserId());

            if (todoItemQuery.Id.HasValue)
            {
                todoItems = todoItems.Where(todoItem => todoItem.Id == todoItemQuery.Id.Value);
            }

            if (!string.IsNullOrWhiteSpace(todoItemQuery.NamePattern))
            {
                todoItems = todoItems.Where(todoItem => EF.Functions.Like(todoItem.Name, todoItemQuery.NamePattern));
            }

            if (todoItemQuery.IsComplete.HasValue)
            {
                todoItems = todoItems.Where(todoItem => todoItem.IsComplete == todoItemQuery.IsComplete.Value);
            }

            return todoItems;
        }

        private IQueryable<TodoItem> SortItems(IQueryable<TodoItem> todoItems, TodoItemQuery todoItemQuery)
        {
            Expression<Func<TodoItem, object>> keySelector = null;

            if (SortByCreatedOn.Equals(todoItemQuery.SortBy, StringComparison.InvariantCultureIgnoreCase))
            {
                keySelector = todoItem => todoItem.CreatedOn;
            }
            else if (SortById.Equals(todoItemQuery.SortBy, StringComparison.InvariantCultureIgnoreCase))
            {
                keySelector = todoItem => todoItem.Id;
            }
            else if (SortByLastUpdatedOn.Equals(todoItemQuery.SortBy, StringComparison.InvariantCultureIgnoreCase))
            {
                keySelector = todoItem => todoItem.LastUpdatedOn;
            }
            else if (SortByName.Equals(todoItemQuery.SortBy, StringComparison.InvariantCultureIgnoreCase))
            {
                keySelector = todoItem => todoItem.Name;
            }

            if (keySelector != null)
            {
                if (todoItemQuery.IsSortAscending.HasValue && !todoItemQuery.IsSortAscending.Value)
                {
                    todoItems = todoItems.OrderByDescending(keySelector);
                }
                else
                {
                    todoItems = todoItems.OrderBy(keySelector);
                }
            }

            return todoItems;
        }

        private IQueryable<TodoItemInfo> ProjectItems(IQueryable<TodoItem> todoItems)
        {
            IQueryable<TodoItemInfo> result = todoItems.Select(todoItem =>
                new TodoItemInfo
                {
                    Id = todoItem.Id,
                    IsComplete = todoItem.IsComplete,
                    Name = todoItem.Name,
                    CreatedBy = todoItem.CreatedBy,
                    CreatedOn = todoItem.CreatedOn,
                    LastUpdatedBy = todoItem.LastUpdatedBy,
                    LastUpdatedOn = todoItem.LastUpdatedOn
                });
            return result;
        }

        private IQueryable<TodoItem> PaginateItems(IQueryable<TodoItem> todoItems,
            TodoItemQuery todoItemQuery)
        {
            IQueryable<TodoItem> result = todoItems.Skip(todoItemQuery.PageIndex).Take(todoItemQuery.PageSize);
            return result;
        }
    }
}