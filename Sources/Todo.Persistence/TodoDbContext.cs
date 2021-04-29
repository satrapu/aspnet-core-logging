namespace Todo.Persistence
{
    using Microsoft.EntityFrameworkCore;

    using Entities;
    using Entities.Configurations;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
        {
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DbSet<TodoItem> TodoItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TodoItemConfiguration());
        }
    }
}
