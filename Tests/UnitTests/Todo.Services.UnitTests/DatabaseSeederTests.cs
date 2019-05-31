using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Todo.Persistence;
using Todo.Services.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Services
{
    /// <summary>
    /// Contains unit tests targeting <see cref="DatabaseSeeder"/> class.
    /// </summary>
    public class DatabaseSeederTests: IClassFixture<TodoDbContextFactory>, IDisposable
    {
        private readonly TodoDbContextFactory todoDbContextFactory;
        private readonly XunitLoggerProvider xunitLoggerProvider;
        private readonly ILogger logger;

        public DatabaseSeederTests(TodoDbContextFactory todoDbContextFactory,  ITestOutputHelper testOutputHelper)
        {
            this.todoDbContextFactory = todoDbContextFactory;
            xunitLoggerProvider = new XunitLoggerProvider(testOutputHelper);
            logger = xunitLoggerProvider.CreateLogger<DatabaseSeederTests>();
        }

        public void Dispose()
        {
            xunitLoggerProvider.Dispose();
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Fact]
        public void Seed_UsingNullAsTodoDbContext_MustThrowException()
        {
            try
            {
                // Arrange
                logger.LogMethodEntered();
                
                TodoDbContext nullTodoDbContext = null;
                var seedingData = Enumerable.Empty<TodoItem>();
                var mockLogger = new Mock<ILogger<DatabaseSeeder>>();
                var databaseSeeder = new DatabaseSeeder(mockLogger.Object);

                // Act 
                // ReSharper disable once ExpressionIsAlwaysNull
                var exception = Record.Exception(() => databaseSeeder.Seed(nullTodoDbContext, seedingData));

                // Assert
                exception.Should()
                         .NotBeNull()
                         .And.BeAssignableTo<ArgumentNullException>()
                         .And.Subject.As<ArgumentNullException>()
                         .ParamName.Should()
                         .Be("todoDbContext");
            }
            finally
            {
                logger.LogMethodExited();
            }
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Fact]
        public void Seed_UsingNullAsSeedingData_MustThrowException()
        {
            try
            {
                // Arrange
                logger.LogMethodEntered();

                var todoDbContext = todoDbContextFactory.TodoDbContext;
                IEnumerable<TodoItem> nullSeedingData = null;
                var mockLogger = new Mock<ILogger<DatabaseSeeder>>();
                var databaseSeeder = new DatabaseSeeder(mockLogger.Object);

                // Act 
                // ReSharper disable once ExpressionIsAlwaysNull
                var exception = Record.Exception(() => databaseSeeder.Seed(todoDbContext, nullSeedingData));

                // Assert
                exception.Should()
                         .NotBeNull()
                         .And.BeAssignableTo<ArgumentNullException>()
                         .And.Subject.As<ArgumentNullException>()
                         .ParamName.Should()
                         .Be("seedingData");
            }
            finally
            {
                logger.LogMethodExited();
            }
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Fact]
        public void Seed_UsingValidArguments_MustReturnTrue()
        {
            try
            {
                // Arrange
                logger.LogMethodEntered();

                var todoDbContext = todoDbContextFactory.TodoDbContext;
                var seedingData = Enumerable.Empty<TodoItem>();
                var mockLogger = new Mock<ILogger<DatabaseSeeder>>();
                var databaseSeeder = new DatabaseSeeder(mockLogger.Object);

                // Act 
                var hasSeededSucceeded = databaseSeeder.Seed(todoDbContext, seedingData);

                // Assert
                hasSeededSucceeded.Should().BeTrue();
            }
            finally
            {
                logger.LogMethodExited();
            }
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Fact]
        public void Seed_WhenThereIsAlreadySomeData_MustReturnFalse()
        {
            try
            {
                // Arrange
                logger.LogMethodEntered();

                using (var todoDbContext = GetTodoDbContextMock())
                {
                    todoDbContext.TodoItems.Add(new TodoItem());
                    todoDbContext.SaveChanges();

                    var seedingData = Enumerable.Empty<TodoItem>();
                    var mockLogger = new Mock<ILogger<DatabaseSeeder>>();
                    var databaseSeeder = new DatabaseSeeder(mockLogger.Object);

                    // Act 
                    var hasSeededSucceeded = databaseSeeder.Seed(todoDbContext, seedingData);

                    // Assert
                    hasSeededSucceeded.Should().BeFalse();
                }
            }
            finally
            {
                logger.LogMethodExited();
            }
        }

        private static TodoDbContext GetTodoDbContextMock()
        {
            var databaseName = $"db-{Guid.NewGuid():N}";
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TodoDbContext>();
            var dbContextOptions = dbContextOptionsBuilder.UseInMemoryDatabase(databaseName: databaseName).Options;

            var result = new TodoDbContext(dbContextOptions);
            return result;
        }
    }
}
