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
        public async IAsyncEnumerable<TodoItemModel> GetByQuery([FromQuery] TodoItemQueryModel todoItemQueryModel)
        {
            var todoItemQuery = new TodoItemQuery
            {
                Id = todoItemQueryModel.Id,
                IsComplete = todoItemQueryModel.IsComplete,
                NamePattern = todoItemQueryModel.NamePattern,
                User = User,
                PageIndex = todoItemQueryModel.PageIndex,
                PageSize = todoItemQueryModel.PageSize,
                IsSortAscending = todoItemQueryModel.IsSortAscending,
                SortBy = todoItemQueryModel.SortBy
            };

            IList<TodoItemInfo> todoItemInfos = await todoService.GetByQueryAsync(todoItemQuery).ConfigureAwait(false);

            foreach (TodoItemInfo todoItemInfo in todoItemInfos)
            {
                yield return new TodoItemModel
                {
                    Id = todoItemInfo.Id,
                    IsComplete = todoItemInfo.IsComplete,
                    Name = todoItemInfo.Name,
                    CreatedBy = todoItemInfo.CreatedBy,
                    CreatedOn = todoItemInfo.CreatedOn,
                    LastUpdatedBy = todoItemInfo.LastUpdatedBy,
                    LastUpdatedOn = todoItemInfo.LastUpdatedOn
                };
            }
        }

        [HttpGet("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.GetTodoItems)]
        public async Task<ActionResult<TodoItemModel>> GetById(long id)
        {
            var todoItemQuery = new TodoItemQuery
            {
                Id = id,
                User = User
            };

            IList<TodoItemInfo> todoItemInfoList =
                await todoService.GetByQueryAsync(todoItemQuery).ConfigureAwait(false);
            TodoItemModel model = todoItemInfoList.Select(todoItemInfo => new TodoItemModel
            {
                Id = todoItemInfo.Id,
                IsComplete = todoItemInfo.IsComplete,
                Name = todoItemInfo.Name,
                CreatedBy = todoItemInfo.CreatedBy,
                CreatedOn = todoItemInfo.CreatedOn,
                LastUpdatedBy = todoItemInfo.LastUpdatedBy,
                LastUpdatedOn = todoItemInfo.LastUpdatedOn
            }).FirstOrDefault();

            if (model == null)
            {
                return NotFound();
            }

            return Ok(model);
        }

        [HttpPost]
        [Authorize(Policy = Policies.TodoItems.CreateTodoItem)]
        public IActionResult Create(NewTodoItemModel newTodoItemModel)
        {
            var newTodoItemInfo = new NewTodoItemInfo
            {
                IsComplete = newTodoItemModel.IsComplete,
                Name = newTodoItemModel.Name,
                User = User
            };

            long newlyCreatedEntityId = todoService.Add(newTodoItemInfo);
            return Created($"api/todo/{newlyCreatedEntityId}", newlyCreatedEntityId);
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.UpdateTodoItem)]
        public IActionResult Update(long id, [FromBody] UpdateTodoItemModel updateTodoItemModel)
        {
            var updateTodoItemInfo = new UpdateTodoItemInfo
            {
                Id = id,
                IsComplete = updateTodoItemModel.IsComplete,
                Name = updateTodoItemModel.Name,
                User = User
            };

            todoService.Update(updateTodoItemInfo);

            return NoContent();
        }

        [HttpDelete("{id:long}")]
        [Authorize(Policy = Policies.TodoItems.DeleteTodoItem)]
        public IActionResult Delete(long id)
        {
            var deleteTodoItemInfo = new DeleteTodoItemInfo
            {
                Id = id,
                User = User
            };

            todoService.Delete(deleteTodoItemInfo);

            return NoContent();
        }
    }
}