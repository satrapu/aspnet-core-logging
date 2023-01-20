namespace Todo.Services.TodoItemManagement
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Security.Principal;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Commons;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using Persistence;
    using Persistence.Entities;

    using Security;

    /// <summary>
    /// An <see cref="ITodoItemService"/> implementation which persists data using Entity Framework Core.
    /// </summary>
    public class TodoItemService : ITodoItemService
    {
        private readonly TodoDbContext todoDbContext;
        private readonly ILogger logger;
        private const string SortByCreatedOn = nameof(TodoItem.CreatedOn);
        private const string SortById = nameof(TodoItem.Id);
        private const string SortByLastUpdatedOn = nameof(TodoItem.LastUpdatedOn);
        private const string SortByName = nameof(TodoItem.Name);
        private static readonly Expression<Func<TodoItem, object>> DefaultKeySelector = todoItem => todoItem.Id;
        private static readonly string TypeFullName = typeof(TodoItemService).FullName;

        /// <summary>
        /// Creates a new instance of the <see cref="TodoItemService"/> class.
        /// </summary>
        /// <param name="todoDbContext">Provides access to the underlying database storing
        /// <see cref="TodoItem"/> instances.</param>
        /// <param name="logger">Provides logging services.</param>
        public TodoItemService(TodoDbContext todoDbContext, ILogger<TodoItemService> logger)
        {
            this.todoDbContext = todoDbContext ?? throw new ArgumentNullException(nameof(todoDbContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<IList<TodoItemInfo>> GetByQueryAsync(TodoItemQuery todoItemQuery)
        {
            using Activity activity = ActivitySources.TodoActivitySource.StartActivity(CreateActivityName());

            activity?.AddEvent(new ActivityEvent("Validate input - BEGIN"));
            ArgumentNullException.ThrowIfNull(todoItemQuery);
            Validator.ValidateObject(todoItemQuery, new ValidationContext(todoItemQuery), validateAllProperties: true);
            activity?.AddEvent(new ActivityEvent("Validate input - END"));

            activity?.AddEvent(new ActivityEvent("Fetch data - BEGIN"));
            Task<IList<TodoItemInfo>> result = InternalGetByQueryAsync(todoItemQuery);
            activity?.AddEvent(new ActivityEvent("Fetch data - END"));

            return result;
        }

        public Task<long> AddAsync(NewTodoItemInfo newTodoItemInfo)
        {
            using Activity _ = ActivitySources.TodoActivitySource.StartActivity(CreateActivityName());

            ArgumentNullException.ThrowIfNull(newTodoItemInfo);

            Validator.ValidateObject(newTodoItemInfo, new ValidationContext(newTodoItemInfo),
                validateAllProperties: true);

            return InternalAddAsync(newTodoItemInfo);
        }

        public Task UpdateAsync(UpdateTodoItemInfo updateTodoItemInfo)
        {
            using Activity _ = ActivitySources.TodoActivitySource.StartActivity(CreateActivityName());

            ArgumentNullException.ThrowIfNull(updateTodoItemInfo);

            Validator.ValidateObject(updateTodoItemInfo, new ValidationContext(updateTodoItemInfo),
                validateAllProperties: true);

            return InternalUpdateAsync(updateTodoItemInfo);
        }

        public Task DeleteAsync(DeleteTodoItemInfo deleteTodoItemInfo)
        {
            using Activity _ = ActivitySources.TodoActivitySource.StartActivity(CreateActivityName());

            ArgumentNullException.ThrowIfNull(deleteTodoItemInfo);

            Validator.ValidateObject(deleteTodoItemInfo, new ValidationContext(deleteTodoItemInfo),
                validateAllProperties: true);

            return InternalDeleteAsync(deleteTodoItemInfo);
        }

        private static string CreateActivityName([CallerMemberName] string callerMemberName = "")
        {
            return $"{TypeFullName}.{callerMemberName}";
        }

        private async Task<IList<TodoItemInfo>> InternalGetByQueryAsync(TodoItemQuery todoItemQuery)
        {
            using Activity activity = ActivitySources.TodoActivitySource.StartActivity(CreateActivityName());
            logger.LogInformation("About to fetch items using query {@TodoItemQuery} ...", todoItemQuery);

            IQueryable<TodoItem> todoItems = FilterItems(todoItemQuery)
                // Read more about query tags here:
                // https://docs.microsoft.com/en-us/ef/core/querying/tags
                .TagWith($"{nameof(TodoItemService)}#{nameof(GetByQueryAsync)}")
                // Read more about no tracking queries here:
                // https://docs.microsoft.com/en-us/ef/core/querying/tracking#no-tracking-queries
                .AsNoTracking();

            todoItems = SortItems(todoItems, todoItemQuery);
            todoItems = PaginateItems(todoItems, todoItemQuery);
            IQueryable<TodoItemInfo> todoItemInfos = ProjectItems(todoItems);
            IList<TodoItemInfo> result = await todoItemInfos.ToListAsync();

            logger.LogInformation("Fetched {TodoItemInfoListCount} todo item(s)", result.Count);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("{@TodoItemInfoList}", result);
            }

            // @satrapu January 15th, 2023: Read more about "Activity.IsAllDataRequested" property here:
            // https://rehansaeed.com/deep-dive-into-open-telemetry-for-net/#isrecording.
            if (activity is not null && activity.IsAllDataRequested)
            {
                activity.AddEvent(new ActivityEvent(name: "Data has been fetched",
                    tags: new ActivityTagsCollection { new KeyValuePair<string, object>("data", JsonSerializer.Serialize(result)) }));
            }

            return result;
        }

        private async Task<long> InternalAddAsync(NewTodoItemInfo newTodoItemInfo)
        {
            logger.LogInformation("About to add item using context {@NewTodoItemInfo} ...", newTodoItemInfo);

            var newTodoItem = new TodoItem(newTodoItemInfo.Name, newTodoItemInfo.Owner.GetName());

            if (newTodoItemInfo.IsComplete.HasValue)
            {
                newTodoItem.IsComplete = newTodoItemInfo.IsComplete.Value;
            }

            await todoDbContext.TodoItems.AddAsync(newTodoItem);
            await todoDbContext.SaveChangesAsync();

            logger.LogInformation("Item with id {TodoItemId} has been added by user [{User}]",
                newTodoItem.Id, newTodoItem.CreatedBy);

            return newTodoItem.Id;
        }

        private async Task InternalUpdateAsync(UpdateTodoItemInfo updateTodoItemInfo)
        {
            logger.LogInformation("About to update item using context {@UpdateTodoItemInfo} ...", updateTodoItemInfo);

            TodoItem existingTodoItem = await GetExistingTodoItem(updateTodoItemInfo.Id, updateTodoItemInfo.Owner);

            if (updateTodoItemInfo.IsComplete.HasValue)
            {
                existingTodoItem.IsComplete = updateTodoItemInfo.IsComplete.Value;
            }

            existingTodoItem.Name = updateTodoItemInfo.Name;
            existingTodoItem.LastUpdatedBy = updateTodoItemInfo.Owner.GetName();
            existingTodoItem.LastUpdatedOn = DateTime.UtcNow;

            todoDbContext.TodoItems.Update(existingTodoItem);
            await todoDbContext.SaveChangesAsync();

            logger.LogInformation("Item with id {TodoItemId} has been updated by user [{User}]",
                existingTodoItem.Id, existingTodoItem.LastUpdatedBy);
        }

        private async Task InternalDeleteAsync(DeleteTodoItemInfo deleteTodoItemInfo)
        {
            logger.LogInformation("About to delete item using context {@DeleteTodoItemInfo} ...", deleteTodoItemInfo);

            TodoItem existingTodoItem = await GetExistingTodoItem(deleteTodoItemInfo.Id, deleteTodoItemInfo.Owner);

            todoDbContext.TodoItems.Remove(existingTodoItem);
            await todoDbContext.SaveChangesAsync();

            logger.LogInformation("Item with id {TodoItemId} has been deleted by user [{User}]",
                deleteTodoItemInfo.Id, deleteTodoItemInfo.Owner.GetName());
        }

        private async Task<TodoItem> GetExistingTodoItem(long? id, IPrincipal owner)
        {
            TodoItem existingTodoItem = await todoDbContext.TodoItems
                .SingleOrDefaultAsync(todoItem => todoItem.Id == id && todoItem.CreatedBy == owner.GetName());

            if (existingTodoItem == null)
            {
                throw new EntityNotFoundException(typeof(TodoItem), id);
            }

            return existingTodoItem;
        }

        private IQueryable<TodoItem> FilterItems(TodoItemQuery todoItemQuery)
        {
            using Activity _ = ActivitySources.TodoActivitySource.StartActivity(CreateActivityName());

            IQueryable<TodoItem> todoItems =
                todoDbContext.TodoItems.Where(todoItem => todoItem.CreatedBy == todoItemQuery.Owner.GetName());

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
            using Activity activity = ActivitySources.TodoActivitySource.StartActivity(CreateActivityName());
            activity?.Tags.Append(new KeyValuePair<string, string>(nameof(todoItemQuery.SortBy), todoItemQuery.SortBy));
            activity?.Tags.Append(new KeyValuePair<string, string>(nameof(todoItemQuery.IsSortAscending), todoItemQuery.IsSortAscending?.ToString()));

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
            using Activity _ = ActivitySources.TodoActivitySource.StartActivity(CreateActivityName());

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
            using Activity activity = ActivitySources.TodoActivitySource.StartActivity(CreateActivityName());
            activity?.Tags.Append(new KeyValuePair<string, string>(nameof(todoItemQuery.PageIndex), todoItemQuery.PageIndex?.ToString()));
            activity?.Tags.Append(new KeyValuePair<string, string>(nameof(todoItemQuery.PageSize), todoItemQuery.PageSize?.ToString()));

            int pageIndex = todoItemQuery.PageIndex.Value;
            int pageSize = todoItemQuery.PageSize.Value;

            IQueryable<TodoItem> result = todoItems.Skip(pageIndex * pageSize).Take(pageSize);
            return result;
        }
    }
}
