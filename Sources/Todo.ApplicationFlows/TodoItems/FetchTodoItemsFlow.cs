namespace Todo.ApplicationFlows.TodoItems
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// An <see cref="IFetchTodoItemsFlow"/> implementation.
    /// </summary>
    public class FetchTodoItemsFlow : TransactionalBaseApplicationFlow<TodoItemQuery, IList<TodoItemInfo>>,
        IFetchTodoItemsFlow
    {
        private readonly ITodoItemService todoItemService;

        public FetchTodoItemsFlow(ITodoItemService todoItemService,
            IOptionsMonitor<ApplicationFlowOptions> applicationFlowOptions,
            ILogger<FetchTodoItemsFlow> logger) :
            base("TodoItems/FetchByQuery", applicationFlowOptions, logger)
        {
            this.todoItemService = todoItemService ?? throw new ArgumentNullException(nameof(todoItemService));
        }

        protected override async Task<IList<TodoItemInfo>> ExecuteFlowStepsAsync(TodoItemQuery input,
            IPrincipal flowInitiator)
        {
            // Ensure that the application fetches data belonging to the current user only (usually the one initiating
            // the current flow).
            input.Owner = flowInitiator;
            return await todoItemService.GetByQueryAsync(input);
        }
    }
}
