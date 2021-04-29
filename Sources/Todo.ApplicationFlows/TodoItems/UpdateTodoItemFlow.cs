namespace Todo.ApplicationFlows.TodoItems
{
    using System;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// An <see cref="IUpdateTodoItemFlow"/> implementation.
    /// </summary>
    public class UpdateTodoItemFlow : TransactionalBaseApplicationFlow<UpdateTodoItemInfo, object>, IUpdateTodoItemFlow
    {
        private readonly ITodoItemService todoItemService;

        public UpdateTodoItemFlow(ITodoItemService todoItemService,
            IOptionsMonitor<ApplicationFlowOptions> applicationFlowOptions, ILogger<UpdateTodoItemFlow> logger) :
            base("TodoItem/Update", applicationFlowOptions, logger)
        {
            this.todoItemService = todoItemService ?? throw new ArgumentNullException(nameof(todoItemService));
        }

        protected override async Task<object> ExecuteFlowStepsAsync(UpdateTodoItemInfo input, IPrincipal flowInitiator)
        {
            input.Owner = flowInitiator;
            await todoItemService.UpdateAsync(input);
            return null;
        }
    }
}
