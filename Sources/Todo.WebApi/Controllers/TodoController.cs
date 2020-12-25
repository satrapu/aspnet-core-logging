using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Todo.Services;
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
        private readonly FetchTodoItemsFlow fetchTodoItemsFlow;
        private readonly ITodoService todoService;
        private readonly ILogger logger;

        public TodoController(ITodoService todoService, FetchTodoItemsFlow fetchTodoItemsFlow,
            ILogger<TodoController> logger)
        {
            this.todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
            this.fetchTodoItemsFlow = fetchTodoItemsFlow ?? throw new ArgumentNullException(nameof(fetchTodoItemsFlow));
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
            // var transactionalApplicationFlow =
            //     new TransactionalApplicationFlow(BusinessFlowNames.Crud.GetTodoItems, User, logger);
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
            using (logger.BeginScope(new Dictionary<string, object>
            {
                [BusinessFlowNames.ScopeKey] = BusinessFlowNames.Crud.GetTodoItem
            }))
            {
                var todoItemQuery = new TodoItemQuery
                {
                    Id = id,
                    Owner = User
                };

                IList<TodoItemInfo> todoItemInfoList =
                    await todoService.GetByQueryAsync(todoItemQuery).ConfigureAwait(false);
                TodoItemModel model = todoItemInfoList.Select(MapFrom).FirstOrDefault();

                if (model == null)
                {
                    return NotFound();
                }

                return Ok(model);
            }
        }

        [HttpPost]
        [Authorize(Policy = Policies.TodoItems.CreateTodoItem)]
        public async Task<IActionResult> CreateAsync(NewTodoItemModel newTodoItemModel)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                [BusinessFlowNames.ScopeKey] = BusinessFlowNames.Crud.CreateTodoItem
            }))
            {
                var newTodoItemInfo = new NewTodoItemInfo
                {
                    IsComplete = newTodoItemModel.IsComplete,
                    Name = newTodoItemModel.Name,
                    User = User
                };

                long newlyCreatedEntityId = await todoService.AddAsync(newTodoItemInfo).ConfigureAwait(false);
                return Created($"api/todo/{newlyCreatedEntityId}", newlyCreatedEntityId);
            }
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.UpdateTodoItem)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateTodoItemModel updateTodoItemModel)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                [BusinessFlowNames.ScopeKey] = BusinessFlowNames.Crud.UpdateTodoItem
            }))
            {
                var updateTodoItemInfo = new UpdateTodoItemInfo
                {
                    Id = id,
                    IsComplete = updateTodoItemModel.IsComplete,
                    Name = updateTodoItemModel.Name,
                    User = User
                };

                await todoService.UpdateAsync(updateTodoItemInfo).ConfigureAwait(false);
                return NoContent();
            }
        }

        [HttpDelete("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.DeleteTodoItem)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                [BusinessFlowNames.ScopeKey] = BusinessFlowNames.Crud.DeleteTodoItem
            }))
            {
                var deleteTodoItemInfo = new DeleteTodoItemInfo
                {
                    Id = id,
                    User = User
                };

                await todoService.DeleteAsync(deleteTodoItemInfo).ConfigureAwait(false);
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