using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Todo.Services.TodoItemLifecycleManagement;

namespace Todo.ApplicationFlows.TodoItems
{
    /// <summary>
    /// An <see cref="IAddTodoItemFlow"/> implementation.
    /// </summary>
    public class AddTodoItemFlow : TransactionalBaseApplicationFlow<NewTodoItemInfo, long>, IAddTodoItemFlow
    {
        private readonly ITodoItemService todoItemService;

        public AddTodoItemFlow(ITodoItemService todoItemService, ILogger<AddTodoItemFlow> logger) :
            base("TodoItem/Add", logger)
        {
            this.todoItemService = todoItemService ?? throw new ArgumentNullException(nameof(todoItemService));
        }

        protected override async Task<long> ExecuteFlowStepsAsync(NewTodoItemInfo input, IPrincipal flowInitiator)
        {
            input.Owner = flowInitiator;
            return await todoItemService.AddAsync(input);
        }
    }
}