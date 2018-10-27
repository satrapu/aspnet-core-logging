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
            return new ActionResult<IList<TodoItem>>(todoItems);
        }

        [HttpGet("{id}", Name = "GetTodo")]
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

            return CreatedAtRoute("GetTodo", new { id = todoItem.Id }, todoItem);
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

            return CreatedAtRoute("GetTodo", new { id = existingTodoItem.Id }, existingTodoItem);
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

            return NoContent();
        }
    }
}