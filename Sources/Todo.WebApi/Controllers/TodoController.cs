namespace Todo.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ApplicationFlows.TodoItems;

    using Authorization;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using Models;

    using Services.TodoItemLifecycleManagement;

    [Route("api/todo")]
    [Authorize]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly IFetchTodoItemsFlow fetchTodoItemsFlow;
        private readonly IFetchTodoItemByIdFlow fetchTodoItemByIdFlow;
        private readonly IAddTodoItemFlow addTodoItemFlow;
        private readonly IUpdateTodoItemFlow updateTodoItemFlow;
        private readonly IDeleteTodoItemFlow deleteTodoItemFlow;

        public TodoController(IFetchTodoItemsFlow fetchTodoItemsFlow,
            IFetchTodoItemByIdFlow fetchTodoItemByIdFlow,
            IAddTodoItemFlow addTodoItemFlow,
            IUpdateTodoItemFlow updateTodoItemFlow,
            IDeleteTodoItemFlow deleteTodoItemFlow)
        {
            this.fetchTodoItemsFlow = fetchTodoItemsFlow ?? throw new ArgumentNullException(nameof(fetchTodoItemsFlow));
            this.fetchTodoItemByIdFlow =
                fetchTodoItemByIdFlow ?? throw new ArgumentNullException(nameof(fetchTodoItemByIdFlow));
            this.addTodoItemFlow = addTodoItemFlow ?? throw new ArgumentNullException(nameof(addTodoItemFlow));
            this.updateTodoItemFlow = updateTodoItemFlow ?? throw new ArgumentNullException(nameof(updateTodoItemFlow));
            this.deleteTodoItemFlow = deleteTodoItemFlow ?? throw new ArgumentNullException(nameof(deleteTodoItemFlow));
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

            IList<TodoItemInfo> todoItemInfos = await fetchTodoItemsFlow.ExecuteAsync(todoItemQuery, User);

            foreach (TodoItemInfo todoItemInfo in todoItemInfos)
            {
                yield return MapFrom(todoItemInfo);
            }
        }

        [HttpGet("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.GetTodoItems)]
        public async Task<ActionResult<TodoItemModel>> GetByIdAsync(long id)
        {
            TodoItemInfo todoItemInfo = await fetchTodoItemByIdFlow.ExecuteAsync(id, User);

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
            var newTodoItemInfo = new NewTodoItemInfo
            {
                IsComplete = newTodoItemModel.IsComplete,
                Name = newTodoItemModel.Name
            };

            long newlyCreatedEntityId = await addTodoItemFlow.ExecuteAsync(newTodoItemInfo, User);
            return Created($"api/todo/{newlyCreatedEntityId}", newlyCreatedEntityId);
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.UpdateTodoItem)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateTodoItemModel updateTodoItemModel)
        {
            var updateTodoItemInfo = new UpdateTodoItemInfo
            {
                Id = id,
                IsComplete = updateTodoItemModel.IsComplete,
                Name = updateTodoItemModel.Name,
                Owner = User
            };

            await updateTodoItemFlow.ExecuteAsync(updateTodoItemInfo, User);
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.DeleteTodoItem)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var deleteTodoItemInfo = new DeleteTodoItemInfo
            {
                Id = id,
                Owner = User
            };

            await deleteTodoItemFlow.ExecuteAsync(deleteTodoItemInfo, User);
            return NoContent();
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
