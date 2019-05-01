using Microsoft.EntityFrameworkCore;

namespace Todo.Persistence
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
        {
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DbSet<TodoItem> TodoItems { get; set; }
    }
}