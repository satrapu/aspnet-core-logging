using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Todo.Services.TodoItemLifecycleManagement;

namespace Todo.ApplicationFlows.TodoItems
{
    /// <summary>
    /// An <see cref="IFetchTodoItemsFlow"/> implementation.
    /// </summary>
    public class FetchTodoItemsFlow : BaseApplicationFlow<TodoItemQuery, IList<TodoItemInfo>>, IFetchTodoItemsFlow
    {
        private readonly ITodoItemService todoItemService;

        public FetchTodoItemsFlow(ITodoItemService todoItemService, ILogger<FetchTodoItemsFlow> logger) :
            base("TodoItems/FetchByQuery", logger)
        {
            this.todoItemService = todoItemService ?? throw new ArgumentNullException(nameof(todoItemService));
        }

        protected override async Task<IList<TodoItemInfo>> ExecuteFlowStepsAsync(TodoItemQuery input,
            IPrincipal flowInitiator)
        {
            return await todoItemService.GetByQueryAsync(input);
        }
    }
}