using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using TodoWebApp.Models;
using TodoWebApp.Services;

namespace TodoWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private const string ROUTE_NAME_FOR_GET_TODO_ITEM = "GetTodo";
        private readonly ITodoService todoService;
        private readonly ILogger logger;

        public TodoController(ITodoService todoService, ILogger<TodoController> logger)
        {
            this.todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public ActionResult<IList<TodoItem>> GetAll()
        {
            var todoItems = todoService.GetAll();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Found {todoItems?.Count} items");
            }

            return new ActionResult<IList<TodoItem>>(todoItems);
        }

        [HttpGet("{id}", Name = ROUTE_NAME_FOR_GET_TODO_ITEM)]
        public ActionResult<TodoItem> GetById(long id)
        {
            ActionResult<TodoItem> result;
            var item = todoService.GetById(id);

            if (item == null)
            {
                result = NotFound();
            }
            else
            {
                result = item;
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Found {result} by id {id}");
            }

            return result;
        }

        [HttpPost]
        public IActionResult Create(TodoItem todoItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            todoService.Add(todoItem);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Created item {todoItem}");
            }

            return CreatedAtRoute(ROUTE_NAME_FOR_GET_TODO_ITEM, new { id = todoItem.Id }, todoItem);
        }

        [HttpPut("{id}")]
        public IActionResult Update(long id, TodoItem todoItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingTodoItem = todoService.GetById(id);

            if (existingTodoItem == null)
            {
                return NotFound();
            }

            existingTodoItem.IsComplete = todoItem.IsComplete;
            existingTodoItem.Name = todoItem.Name;
            todoService.Update(existingTodoItem);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Updated item with id {id}");
            }

            return CreatedAtRoute(ROUTE_NAME_FOR_GET_TODO_ITEM, new { id = existingTodoItem.Id }, existingTodoItem);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            var todoItem = todoService.GetById(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            todoService.Delete(todoItem);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Deleted item with id {id}");
            }

            return NoContent();
        }
    }
}