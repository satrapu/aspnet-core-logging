using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Todo.ApplicationFlows.TodoItems;
using Todo.Services.TodoItemLifecycleManagement;
using Todo.WebApi.Authorization;
using Todo.WebApi.Logging;
using Todo.WebApi.Models;

namespace Todo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly IFetchTodoItemsFlow fetchTodoItemsFlow;
        private readonly IFetchTodoItemByIdFlow fetchTodoItemByIdFlow;
        private readonly ITodoItemService todoItemService;
        private readonly ILogger logger;

        public TodoController(ITodoItemService todoItemService,
            IFetchTodoItemsFlow fetchTodoItemsFlow,
            IFetchTodoItemByIdFlow fetchTodoItemByIdFlow,
            ILogger<TodoController> logger)
        {
            this.todoItemService = todoItemService ?? throw new ArgumentNullException(nameof(todoItemService));
            this.fetchTodoItemsFlow = fetchTodoItemsFlow ?? throw new ArgumentNullException(nameof(fetchTodoItemsFlow));
            this.fetchTodoItemByIdFlow =
                fetchTodoItemByIdFlow ?? throw new ArgumentNullException(nameof(fetchTodoItemByIdFlow));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Authorize(Policy = Policies.TodoItems.GetTodoItems)]
        public async IAsyncEnumerable<TodoItemModel> GetByQueryAsync([FromQuery] TodoItemQueryModel todoItemQueryModel)
        {
            var todoItemQuery = new TodoItemQuery
            {
                Id = todoItemQueryModel.Id,
                IsComplete = todoItemQueryModel.IsComplete,
                NamePattern = todoItemQueryModel.NamePattern,
                Owner = User,
                PageIndex = todoItemQueryModel.PageIndex,
                PageSize = todoItemQueryModel.PageSize,
                IsSortAscending = todoItemQueryModel.IsSortAscending,
                SortBy = todoItemQueryModel.SortBy
            };

            IList<TodoItemInfo> todoItemInfos =
                await fetchTodoItemsFlow.ExecuteAsync(todoItemQuery, User).ConfigureAwait(false);

            foreach (TodoItemInfo todoItemInfo in todoItemInfos)
            {
                yield return MapFrom(todoItemInfo);
            }
        }

        [HttpGet("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.GetTodoItems)]
        public async Task<ActionResult<TodoItemModel>> GetByIdAsync(long id)
        {
            TodoItemInfo todoItemInfo = await fetchTodoItemByIdFlow.ExecuteAsync(id, User).ConfigureAwait(false);

            if (todoItemInfo == null)
            {
                return NotFound();
            }

            TodoItemModel result = MapFrom(todoItemInfo);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Policies.TodoItems.CreateTodoItem)]
        public async Task<IActionResult> CreateAsync(NewTodoItemModel newTodoItemModel)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                [ApplicationFlowNames.ScopeKey] = ApplicationFlowNames.Crud.CreateTodoItem
            }))
            {
                var newTodoItemInfo = new NewTodoItemInfo
                {
                    IsComplete = newTodoItemModel.IsComplete,
                    Name = newTodoItemModel.Name,
                    User = User
                };

                long newlyCreatedEntityId = await todoItemService.AddAsync(newTodoItemInfo).ConfigureAwait(false);
                return Created($"api/todo/{newlyCreatedEntityId}", newlyCreatedEntityId);
            }
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.UpdateTodoItem)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateTodoItemModel updateTodoItemModel)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                [ApplicationFlowNames.ScopeKey] = ApplicationFlowNames.Crud.UpdateTodoItem
            }))
            {
                var updateTodoItemInfo = new UpdateTodoItemInfo
                {
                    Id = id,
                    IsComplete = updateTodoItemModel.IsComplete,
                    Name = updateTodoItemModel.Name,
                    User = User
                };

                await todoItemService.UpdateAsync(updateTodoItemInfo).ConfigureAwait(false);
                return NoContent();
            }
        }

        [HttpDelete("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.DeleteTodoItem)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                [ApplicationFlowNames.ScopeKey] = ApplicationFlowNames.Crud.DeleteTodoItem
            }))
            {
                var deleteTodoItemInfo = new DeleteTodoItemInfo
                {
                    Id = id,
                    User = User
                };

                await todoItemService.DeleteAsync(deleteTodoItemInfo).ConfigureAwait(false);
                return NoContent();
            }
        }

        private static TodoItemModel MapFrom(TodoItemInfo todoItemInfo)
        {
            TodoItemModel result = new TodoItemModel
            {
                Id = todoItemInfo.Id,
                IsComplete = todoItemInfo.IsComplete,
                Name = todoItemInfo.Name,
                CreatedBy = todoItemInfo.CreatedBy,
                CreatedOn = todoItemInfo.CreatedOn,
                LastUpdatedBy = todoItemInfo.LastUpdatedBy,
                LastUpdatedOn = todoItemInfo.LastUpdatedOn
            };

            return result;
        }
    }
}