using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Services;
using Todo.WebApi.Authorization;
using Todo.WebApi.Models;

namespace Todo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService todoService;

        public TodoController(ITodoService todoService)
        {
            this.todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
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

            IList<TodoItemInfo> todoItemInfos = await todoService.GetByQueryAsync(todoItemQuery).ConfigureAwait(false);

            foreach (TodoItemInfo todoItemInfo in todoItemInfos)
            {
                yield return MapFrom(todoItemInfo);
            }
        }

        [HttpGet("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.GetTodoItems)]
        public async Task<ActionResult<TodoItemModel>> GetByIdAsync(long id)
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

        [HttpPost]
        [Authorize(Policy = Policies.TodoItems.CreateTodoItem)]
        public async Task<IActionResult> CreateAsync(NewTodoItemModel newTodoItemModel)
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

        [HttpPut("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.UpdateTodoItem)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateTodoItemModel updateTodoItemModel)
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

        [HttpDelete("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.DeleteTodoItem)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var deleteTodoItemInfo = new DeleteTodoItemInfo
            {
                Id = id,
                User = User
            };

            await todoService.DeleteAsync(deleteTodoItemInfo).ConfigureAwait(false);
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