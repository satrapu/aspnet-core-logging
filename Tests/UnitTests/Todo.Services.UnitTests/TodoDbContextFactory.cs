using Microsoft.EntityFrameworkCore;
using System;
using Todo.Persistence;

namespace Todo.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    #pragma warning disable S3881 // "IDisposable" should be implemented correctly
    public class TodoDbContextFactory: IDisposable
    #pragma warning restore S3881 // "IDisposable" should be implemented correctly
    {
        public TodoDbContextFactory()
        {
            TodoDbContext = GetTodoDbContextMock();
        }

        public TodoDbContext TodoDbContext { get; }

        private static TodoDbContext GetTodoDbContextMock()
        {
            var databaseName = $"db-{Guid.NewGuid():N}";
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TodoDbContext>();
            var dbContextOptions = dbContextOptionsBuilder.UseInMemoryDatabase(databaseName: databaseName).Options;

            var result = new TodoDbContext(dbContextOptions);
            return result;
        }

        public void Dispose()
        {
            TodoDbContext?.Dispose();
        }
    }
}