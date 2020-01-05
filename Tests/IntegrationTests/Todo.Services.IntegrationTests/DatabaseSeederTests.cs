using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Linq;
using Todo.Persistence;

namespace Todo.Services
{
    /// <summary>
    /// Contains integration tests targeting <see cref="DatabaseSeeder"/> class.
    /// </summary>
    [TestFixture]
    public class DatabaseSeederTests
    {
        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Test]
        public void Seed_UsingValidArguments_MustReturnTrue()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var dbContextOptions = new DbContextOptionsBuilder<TodoDbContext>().UseInMemoryDatabase(dbName).Options;

            using (var todoDbContext = new TodoDbContext(dbContextOptions))
            {
                var loggerFactory = new NullLoggerFactory();
                var logger = loggerFactory.CreateLogger<DatabaseSeeder>();
                var databaseSeeder = new DatabaseSeeder(logger);
                var seedingData = Enumerable.Empty<TodoItem>();

                // Act 
                var hasSeededSucceeded = databaseSeeder.Seed(todoDbContext, seedingData);

                // Assert
                hasSeededSucceeded.Should().BeTrue();
            }
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Test]
        public void Seed_WhenThereIsAlreadySomeData_MustReturnFalse()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var dbContextOptions = new DbContextOptionsBuilder<TodoDbContext>().UseInMemoryDatabase(dbName).Options;

            using (var todoDbContext = new TodoDbContext(dbContextOptions))
            {
                todoDbContext.TodoItems.Add(new TodoItem
                {
                    Name = Guid.NewGuid().ToString("N"),
                    CreatedBy = "integration-tests",
                    CreatedOn = DateTime.Now,
                    IsComplete = false
                });
                todoDbContext.SaveChanges();
            }

            using (var todoDbContext = new TodoDbContext(dbContextOptions))
            {
                var loggerFactory = new NullLoggerFactory();
                var logger = loggerFactory.CreateLogger<DatabaseSeeder>();
                var databaseSeeder = new DatabaseSeeder(logger);
                var seedingData = Enumerable.Range(1, 5).Select(index => new TodoItem
                {
                    Name = $"{Guid.NewGuid():N}",
                    CreatedBy = "integration-tests",
                    CreatedOn = DateTime.Now,
                    IsComplete = false
                });

                // Act 
                var hasSeededSucceeded = databaseSeeder.Seed(todoDbContext, seedingData);

                // Assert
                hasSeededSucceeded.Should().BeFalse();
            }
        }
    }
}