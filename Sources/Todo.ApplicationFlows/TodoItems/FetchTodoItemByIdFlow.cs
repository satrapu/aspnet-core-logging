using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Todo.Services.TodoItemLifecycleManagement;

namespace Todo.ApplicationFlows.TodoItems
{
    /// <summary>
    /// An <see cref="IFetchTodoItemByIdFlow"/> implementation.
    /// </summary>
    public class FetchTodoItemByIdFlow : BaseApplicationFlow<long, TodoItemInfo>, IFetchTodoItemByIdFlow
    {
        private readonly ITodoItemService todoItemService;

        public FetchTodoItemByIdFlow(ITodoItemService todoItemService, ILogger<FetchTodoItemsFlow> logger) :
            base("TodoItem/FetchById", logger, validateInput: false)
        {
            this.todoItemService = todoItemService ?? throw new ArgumentNullException(nameof(todoItemService));
        }

        protected override async Task<TodoItemInfo> ExecuteFlowStepsAsync(long input,
            IPrincipal flowInitiator)
        {
            var todoItemQuery = new TodoItemQuery
            {
                Id = input,
                Owner = flowInitiator
            };

            IList<TodoItemInfo> todoItemInfos =
                await todoItemService.GetByQueryAsync(todoItemQuery).ConfigureAwait(false);
            TodoItemInfo result = todoItemInfos.FirstOrDefault();
            return result;
        }
    }
}