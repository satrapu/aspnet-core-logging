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
        private static readonly Expression<Func<TodoItem, object>> DefaultKeySelector = todoItem => todoItem.Id;

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

            IQueryable<TodoItem> todoItems = FilterItems(todoItemQuery)
                // Read more about query tags here: 
                // https://docs.microsoft.com/en-us/ef/core/querying/tags
                .TagWith($"{nameof(TodoService)}#{nameof(GetByQueryAsync)}")
                // Read more about no tracking queries here:
                // https://docs.microsoft.com/en-us/ef/core/querying/tracking#no-tracking-queries
                .AsNoTracking();
            todoItems = SortItems(todoItems, todoItemQuery);
            todoItems = PaginateItems(todoItems, todoItemQuery);
            IQueryable<TodoItemInfo> todoItemInfos = ProjectItems(todoItems);
            IList<TodoItemInfo> result = await todoItemInfos.ToListAsync().ConfigureAwait(false);

            logger.LogInformation("Fetched {TodoItemsCount} todo item(s) for user {UserId} using query {TodoItemQuery}",
                result.Count, todoItemQuery.Owner.GetUserId(), todoItemQuery);

            return result;
        }

        public async Task<long> AddAsync(NewTodoItemInfo newTodoItemInfo)
        {
            Validator.ValidateObject(newTodoItemInfo, new ValidationContext(newTodoItemInfo), true);

            var newTodoItem = new TodoItem(newTodoItemInfo.Name, newTodoItemInfo.User.GetUserId())
            {
                // ReSharper disable once PossibleInvalidOperationException
                IsComplete = newTodoItemInfo.IsComplete.Value
            };

            await todoDbContext.TodoItems.AddAsync(newTodoItem);
            await todoDbContext.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("Item with id {TodoItemId} has been added by user {UserId}"
                , newTodoItem.Id, newTodoItem.CreatedBy);

            return newTodoItem.Id;
        }

        public async Task UpdateAsync(UpdateTodoItemInfo updateTodoItemInfo)
        {
            Validator.ValidateObject(updateTodoItemInfo, new ValidationContext(updateTodoItemInfo), true);

            TodoItem existingTodoItem = await GetExistingTodoItem(updateTodoItemInfo.Id, updateTodoItemInfo.User)
                .ConfigureAwait(false);
            // ReSharper disable once PossibleInvalidOperationException
            existingTodoItem.IsComplete = updateTodoItemInfo.IsComplete.Value;
            existingTodoItem.Name = updateTodoItemInfo.Name;
            existingTodoItem.LastUpdatedBy = updateTodoItemInfo.User.GetUserId();
            existingTodoItem.LastUpdatedOn = DateTime.UtcNow;

            todoDbContext.TodoItems.Update(existingTodoItem);
            await todoDbContext.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("Item with id {TodoItemId} has been updated by user {UserId}"
                , existingTodoItem.Id, existingTodoItem.LastUpdatedBy);
        }

        public async Task DeleteAsync(DeleteTodoItemInfo deleteTodoItemInfo)
        {
            Validator.ValidateObject(deleteTodoItemInfo, new ValidationContext(deleteTodoItemInfo), true);

            TodoItem existingTodoItem = await GetExistingTodoItem(deleteTodoItemInfo.Id, deleteTodoItemInfo.User)
                .ConfigureAwait(false);

            todoDbContext.TodoItems.Remove(existingTodoItem);
            await todoDbContext.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("Item with id {TodoItemId} has been deleted by user {UserId}"
                , deleteTodoItemInfo.Id, deleteTodoItemInfo.User.GetUserId());
        }

        private async Task<TodoItem> GetExistingTodoItem(long? id, ClaimsPrincipal owner)
        {
            TodoItem existingTodoItem = await todoDbContext.TodoItems.SingleOrDefaultAsync(todoItem =>
                todoItem.Id == id && todoItem.CreatedBy == owner.GetUserId()).ConfigureAwait(false);

            if (existingTodoItem == null)
            {
                throw new EntityNotFoundException(typeof(TodoItem), id);
            }

            return existingTodoItem;
        }

        private IQueryable<TodoItem> FilterItems(TodoItemQuery todoItemQuery)
        {
            IQueryable<TodoItem> todoItems =
                todoDbContext.TodoItems.Where(todoItem => todoItem.CreatedBy == todoItemQuery.Owner.GetUserId());

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

        private static IQueryable<TodoItem> SortItems(IQueryable<TodoItem> todoItems, TodoItemQuery todoItemQuery)
        {
            Expression<Func<TodoItem, object>> keySelector = GetKeySelectorBy(todoItemQuery.SortBy);

            if (todoItemQuery.IsSortAscending.HasValue && !todoItemQuery.IsSortAscending.Value)
            {
                todoItems = todoItems.OrderByDescending(keySelector);
            }
            else
            {
                todoItems = todoItems.OrderBy(keySelector);
            }

            return todoItems;
        }

        private static Expression<Func<TodoItem, object>> GetKeySelectorBy(string sortByProperty)
        {
            if (string.IsNullOrWhiteSpace(sortByProperty))
            {
                return DefaultKeySelector;
            }

            if (SortByCreatedOn.Equals(sortByProperty, StringComparison.InvariantCultureIgnoreCase))
            {
                return todoItem => todoItem.CreatedOn;
            }

            if (SortById.Equals(sortByProperty, StringComparison.InvariantCultureIgnoreCase))
            {
                return todoItem => todoItem.Id;
            }

            if (SortByLastUpdatedOn.Equals(sortByProperty, StringComparison.InvariantCultureIgnoreCase))
            {
                return todoItem => todoItem.LastUpdatedOn;
            }

            if (SortByName.Equals(sortByProperty, StringComparison.InvariantCultureIgnoreCase))
            {
                return todoItem => todoItem.Name;
            }

            return DefaultKeySelector;
        }

        private static IQueryable<TodoItemInfo> ProjectItems(IQueryable<TodoItem> todoItems)
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

        private static IQueryable<TodoItem> PaginateItems(IQueryable<TodoItem> todoItems, TodoItemQuery todoItemQuery)
        {
            IQueryable<TodoItem> result = todoItems;
            int pageIndex = TodoItemQuery.DefaultPageIndex;
            int pageSize = TodoItemQuery.DefaultPageSize;

            if (todoItemQuery.PageIndex.HasValue)
            {
                pageIndex = todoItemQuery.PageIndex.Value;
            }

            if (todoItemQuery.PageSize.HasValue)
            {
                pageSize = todoItemQuery.PageSize.Value;
            }

            result = result.Skip(pageIndex * pageSize).Take(pageSize);
            return result;
        }
    }
}