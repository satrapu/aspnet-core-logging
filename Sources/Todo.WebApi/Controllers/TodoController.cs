using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Todo.Services;
using Todo.WebApi.Models;

namespace Todo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService todoService;
        private readonly ILogger logger;

        public TodoController(ITodoService todoService, ILogger<TodoController> logger)
        {
            this.todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public ActionResult<IList<TodoItemModel>> GetByQuery([FromQuery]TodoItemQueryModel todoItemQueryModel)
        {
            var todoItemQuery = new TodoItemQuery
            {
                Id = todoItemQueryModel.Id
              , IsComplete = todoItemQueryModel.IsComplete
              , NamePattern = todoItemQueryModel.NamePattern
              , User = User
            };
            var todoItemInfoList = todoService.GetByQuery(todoItemQuery);
            var models = todoItemInfoList.Select(x => new TodoItemModel
            {
                Id = x.Id
              , IsComplete = x.IsComplete
              , Name = x.Name
            }).ToList();

            return models;
        }

        [HttpPost]
        public ActionResult<TodoItemModel> Create(NewTodoItemModel newTodoItemModel)
        {
            var newTodoItemInfo = new NewTodoItemInfo
            {
                IsComplete = newTodoItemModel.IsComplete
              , Name = newTodoItemModel.Name
              , User = User
            };

            var newlyCreatedEntityId = todoService.Add(newTodoItemInfo);
            logger.LogInformation("User with id {UserId} has created a new item with id {ItemId}"
                                , User.GetUserId(), newlyCreatedEntityId);

            var query = new TodoItemQuery
            {
                Id = newlyCreatedEntityId
              , User = User
            };
            var todoItemInfo = todoService.GetByQuery(query).Single();
            return Created(string.Empty, todoItemInfo);
        }

        [HttpPut("{id}")]
        public IActionResult Update(long id, [FromBody]UpdateTodoItemModel updateTodoItemModel)
        {
            var updateTodoItemInfo = new UpdateTodoItemInfo
            {
                Id = id
              , IsComplete = updateTodoItemModel.IsComplete
              , Name = updateTodoItemModel.Name
              , User = User
            };

            todoService.Update(updateTodoItemInfo);
            logger.LogInformation("User with id {UserId} has updated an existing item with id {ItemId}"
                                , User.GetUserId(), id);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            var deleteTodoItemInfo = new DeleteTodoItemInfo
            {
                Id = id, User = User
            };

            todoService.Delete(deleteTodoItemInfo);
            logger.LogInformation("User with id {UserId} has deleted item with id {ItemId}"
                                , User.GetUserId(), id);

            return NoContent();
        }
    }
}