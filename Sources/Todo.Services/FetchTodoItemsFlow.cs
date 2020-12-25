using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Todo.Services
{
    /// <summary>
    /// Application flow used for fetching <see cref="TodoItemInfo"/> instances matching a given query.
    /// </summary>
    public class FetchTodoItemsFlow : BaseApplicationFlow<TodoItemQuery, IList<TodoItemInfo>>
    {
        private readonly ITodoService todoService;
        public FetchTodoItemsFlow(ITodoService todoService, ILogger<FetchTodoItemsFlow> logger) :
            base("Crud/FetchItems", logger)
        {
            this.todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
        }

        protected override async Task<IList<TodoItemInfo>> ExecuteFlowStepsAsync(TodoItemQuery input)
        {
            return await todoService.GetByQueryAsync(input);
        }
    }
}