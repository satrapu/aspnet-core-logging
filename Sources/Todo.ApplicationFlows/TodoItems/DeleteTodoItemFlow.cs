namespace Todo.ApplicationFlows.TodoItems
{
    using System;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Services.TodoItemLifecycleManagement;

    /// <summary>
    /// An <see cref="IDeleteTodoItemFlow"/> implementation.
    /// </summary>
    public class DeleteTodoItemFlow : TransactionalBaseApplicationFlow<DeleteTodoItemInfo, object>, IDeleteTodoItemFlow
    {
        private readonly ITodoItemService todoItemService;

        public DeleteTodoItemFlow(ITodoItemService todoItemService,
            IOptionsMonitor<ApplicationFlowOptions> applicationFlowOptions, ILogger<DeleteTodoItemFlow> logger) :
            base("TodoItem/Delete", applicationFlowOptions, logger)
        {
            this.todoItemService = todoItemService ?? throw new ArgumentNullException(nameof(todoItemService));
        }

        protected override async Task<object> ExecuteFlowStepsAsync(DeleteTodoItemInfo input, IPrincipal flowInitiator)
        {
            input.Owner = flowInitiator;
            await todoItemService.DeleteAsync(input);
            return null;
        }
    }
}
