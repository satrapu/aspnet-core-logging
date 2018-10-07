using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TodoWebApp.Models;

namespace TodoWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly TodoDbContext todoDbContext;
        private readonly ILogger<TodoController> logger;

        public TodoController(TodoDbContext todoDbContext, ILogger<TodoController> logger)
        {
            this.todoDbContext = todoDbContext;
            this.logger = logger;

            if (this.logger.IsEnabled(LogLevel.Debug))
            {
                this.logger.LogDebug(new EventId(0, "Constructor"), $"{MethodBase.GetCurrentMethod().Name}");
            }

            if (this.todoDbContext.TodoItems.Any())
            {
                return;
            }

            // Create a new TodoItem if collection is empty,
            // which means you can't delete all TodoItems.
            this.todoDbContext.TodoItems.Add(new TodoItem { Name = "Item1" });
            this.todoDbContext.SaveChanges();
        }

        [HttpGet]
        public ActionResult<List<TodoItem>> GetAll()
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(new EventId(1, "MethodCall"), $"{MethodBase.GetCurrentMethod().Name} - BEGIN");
            }

            var result = todoDbContext.TodoItems.ToList();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(new EventId(1, "MethodCall"), $"{MethodBase.GetCurrentMethod().Name} - END; Result={result.Count} {nameof(TodoItem)} instance(s)");
            }

            return result;
        }

        [HttpGet("{id}", Name = "GetTodo")]
        public ActionResult<TodoItem> GetById(long id)
        {
            ActionResult<TodoItem> result = null;

            try
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(new EventId(1, "MethodCall"), $"{MethodBase.GetCurrentMethod().Name} - BEGIN");
                    logger.LogDebug(new EventId(1, "MethodCall"), $"Parameter {nameof(id)}={id}");
                }

                var item = todoDbContext.TodoItems.Find(id);

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
            finally
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(new EventId(1, "MethodCall"), $"{MethodBase.GetCurrentMethod().Name} - END; Result={result}");
                }
            }
        }

        [HttpPost]
        public IActionResult Create(TodoItem item)
        {
            todoDbContext.TodoItems.Add(item);
            todoDbContext.SaveChanges();

            return CreatedAtRoute("GetTodo", new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public IActionResult Update(long id, TodoItem item)
        {
            var existingItem = todoDbContext.TodoItems.Find(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.IsComplete = item.IsComplete;
            existingItem.Name = item.Name;

            todoDbContext.TodoItems.Update(existingItem);
            todoDbContext.SaveChanges();

            return CreatedAtRoute("GetTodo", new { id = item.Id }, item);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            var todo = todoDbContext.TodoItems.Find(id);

            if (todo == null)
            {
                return NotFound();
            }

            todoDbContext.TodoItems.Remove(todo);
            todoDbContext.SaveChanges();

            return NoContent();
        }
    }
}