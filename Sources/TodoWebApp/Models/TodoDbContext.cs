using Microsoft.EntityFrameworkCore;

namespace TodoWebApp.Models
{
    /// <summary>
    /// Persists <see cref="TodoItem"/> instances.
    /// </summary>
    public class TodoDbContext : DbContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TodoDbContext"/> class.
        /// </summary>
        /// <param name="options">Configures the access to the underlying database</param>
        public TodoDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the set of <see cref="TodoItem"/> stored inside the underlying database.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DbSet<TodoItem> TodoItems { get; set; }
    }
}