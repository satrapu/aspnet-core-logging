namespace Todo.Persistence
{
    using System;

    using Entities;
    using Entities.Configurations;

    using Microsoft.EntityFrameworkCore;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class TodoDbContext : DbContext
    {
        static TodoDbContext()
        {
            // satrapu 2021-12-02: Temporarily disable a breaking change introduced when migrating
            // Npgsql.EntityFrameworkCore.PostgreSQL NuGet package from v5.x to v6.x.
            // See more about this breaking change and its fix here:
            // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic.
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

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
