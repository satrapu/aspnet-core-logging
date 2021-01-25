using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Todo.Services.TodoItemLifecycleManagement;

namespace Todo.ApplicationFlows.TodoItems
{
    /// <summary>
    /// An <see cref="IUpdateTodoItemFlow"/> implementation.
    /// </summary>
    public class UpdateTodoItemFlow : TransactionalBaseApplicationFlow<UpdateTodoItemInfo, object>, IUpdateTodoItemFlow
    {
        private readonly ITodoItemService todoItemService;

        public UpdateTodoItemFlow(ITodoItemService todoItemService, ILogger<UpdateTodoItemFlow> logger) :
            base("TodoItem/Update", logger)
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