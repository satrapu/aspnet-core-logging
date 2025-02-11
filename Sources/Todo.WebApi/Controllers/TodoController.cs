namespace Todo.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using ApplicationFlows.TodoItems;

    using Authorization;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;

    using Models;

    using Services.TodoItemManagement;

    [Route("api/todo")]
    [Authorize]
    [ApiController]
    [SuppressMessage(category: "Major Code Smell", checkId: "S6960:Controllers should not have mixed responsibilities",
        Justification = "This controller will be replaced at some point with several endpoints")]
    public class TodoController : ControllerBase
    {
        private readonly IFetchTodoItemsFlow fetchTodoItemsFlow;
        private readonly IFetchTodoItemByIdFlow fetchTodoItemByIdFlow;
        private readonly IAddTodoItemFlow addTodoItemFlow;
        private readonly IUpdateTodoItemFlow updateTodoItemFlow;
        private readonly IDeleteTodoItemFlow deleteTodoItemFlow;
        private readonly LinkGenerator linkGenerator;

        public TodoController
        (
            IFetchTodoItemsFlow fetchTodoItemsFlow,
            IFetchTodoItemByIdFlow fetchTodoItemByIdFlow,
            IAddTodoItemFlow addTodoItemFlow,
            IUpdateTodoItemFlow updateTodoItemFlow,
            IDeleteTodoItemFlow deleteTodoItemFlow,
            LinkGenerator linkGenerator
        )
        {
            this.fetchTodoItemsFlow = fetchTodoItemsFlow ?? throw new ArgumentNullException(nameof(fetchTodoItemsFlow));

            this.fetchTodoItemByIdFlow =
                fetchTodoItemByIdFlow ?? throw new ArgumentNullException(nameof(fetchTodoItemByIdFlow));

            this.addTodoItemFlow = addTodoItemFlow ?? throw new ArgumentNullException(nameof(addTodoItemFlow));
            this.updateTodoItemFlow = updateTodoItemFlow ?? throw new ArgumentNullException(nameof(updateTodoItemFlow));
            this.deleteTodoItemFlow = deleteTodoItemFlow ?? throw new ArgumentNullException(nameof(deleteTodoItemFlow));
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        [Authorize(Policy = Policies.TodoItems.GetTodoItems)]
        public async IAsyncEnumerable<TodoItemModel> GetByQueryAsync([FromQuery] TodoItemQueryModel todoItemQueryModel)
        {
            TodoItemQuery todoItemQuery = new()
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
            NewTodoItemInfo newTodoItemInfo = new()
            {
                IsComplete = newTodoItemModel.IsComplete,
                Name = newTodoItemModel.Name
            };

            long newlyCreatedEntityId = await addTodoItemFlow.ExecuteAsync(newTodoItemInfo, User);

            string getNewlyCreatedEntityUri = linkGenerator.GetUriByAction
            (
                httpContext: HttpContext,
                action: "GetById",
                values: new
                {
                    id = newlyCreatedEntityId
                }
            );

            return Created(getNewlyCreatedEntityUri, value: null);
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.UpdateTodoItem)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateTodoItemModel updateTodoItemModel)
        {
            UpdateTodoItemInfo updateTodoItemInfo = new()
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
            DeleteTodoItemInfo deleteTodoItemInfo = new()
            {
                Id = id,
                Owner = User
            };

            await deleteTodoItemFlow.ExecuteAsync(deleteTodoItemInfo, User);

            return NoContent();
        }

        private static TodoItemModel MapFrom(TodoItemInfo todoItemInfo)
        {
            TodoItemModel result = new()
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
