namespace Todo.ApplicationFlows.TodoItems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// An <see cref="IFetchTodoItemByIdFlow"/> implementation.
    /// </summary>
    public class FetchTodoItemByIdFlow : TransactionalBaseApplicationFlow<long, TodoItemInfo>, IFetchTodoItemByIdFlow
    {
        private readonly ITodoItemService todoItemService;

        public FetchTodoItemByIdFlow(ITodoItemService todoItemService,
            IOptionsMonitor<ApplicationFlowOptions> applicationFlowOptions, ILogger<FetchTodoItemByIdFlow> logger) :
            base("TodoItem/FetchById", applicationFlowOptions, logger)
        {
            this.todoItemService = todoItemService ?? throw new ArgumentNullException(nameof(todoItemService));
        }

        protected override async Task<TodoItemInfo> ExecuteFlowStepsAsync(long input, IPrincipal flowInitiator)
        {
            var todoItemQuery = new TodoItemQuery
            {
                Id = input,
                // Ensure that the application fetches data belonging to the current user only (usually the one
                // initiating the current flow).
                Owner = flowInitiator
            };

            IList<TodoItemInfo> todoItemInfos = await todoItemService.GetByQueryAsync(todoItemQuery);
            TodoItemInfo result = todoItemInfos.FirstOrDefault();
            return result;
        }
    }
}
