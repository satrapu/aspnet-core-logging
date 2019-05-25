using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Todo.Persistence;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Services.UnitTests
{
    /// <summary>
    /// Contains unit tests targeting <see cref="DatabaseSeeder"/> class.
    /// </summary>
    public class DatabaseSeederTests: IClassFixture<TodoDbContextFactory>
    {
        private readonly TodoDbContextFactory todoDbContextFactory;
        private readonly ITestOutputHelper testOutputHelper;

        public DatabaseSeederTests(TodoDbContextFactory todoDbContextFactory,  ITestOutputHelper testOutputHelper)
        {
            this.todoDbContextFactory = todoDbContextFactory;
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Fact]
        public void Seed_UsingNullAsTodoDbContext_MustThrowException()
        {
            // Arrange
            testOutputHelper.WriteLine($"Running test method: {GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");
            TodoDbContext nullTodoDbContext = null;
            var seedingData = Enumerable.Empty<TodoItem>();
            var logger = new Mock<ILogger<DatabaseSeeder>>().Object;
            var databaseSeeder = new DatabaseSeeder(logger);

            // Act 
            // ReSharper disable once ExpressionIsAlwaysNull
            var exception = Record.Exception(() => databaseSeeder.Seed(nullTodoDbContext, seedingData));

            // Assert
            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>()
                     .And.Subject.As<ArgumentNullException>().ParamName.Should().Be("todoDbContext");
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Fact]
        public void Seed_UsingNullAsSeedingData_MustThrowException()
        {
            // Arrange
            testOutputHelper.WriteLine($"Running test method: {GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");
            var todoDbContext = todoDbContextFactory.TodoDbContext;
            IEnumerable<TodoItem> nullSeedingData = null;
            var logger = new Mock<ILogger<DatabaseSeeder>>().Object;
            var databaseSeeder = new DatabaseSeeder(logger);

            // Act 
            // ReSharper disable once ExpressionIsAlwaysNull
            var exception = Record.Exception(() => databaseSeeder.Seed(todoDbContext, nullSeedingData));

            // Assert
            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>()
                     .And.Subject.As<ArgumentNullException>().ParamName.Should().Be("seedingData");
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Fact]
        public void Seed_UsingValidArguments_MustReturnTrue()
        {
            // Arrange
            testOutputHelper.WriteLine($"Running test method: {GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");
            var todoDbContext = todoDbContextFactory.TodoDbContext;
            var seedingData = Enumerable.Empty<TodoItem>();
            var logger = new Mock<ILogger<DatabaseSeeder>>().Object;
            var databaseSeeder = new DatabaseSeeder(logger);

            // Act 
            var hasSeededSucceeded = databaseSeeder.Seed(todoDbContext, seedingData);

            // Assert
            hasSeededSucceeded.Should().BeTrue();
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Fact]
        public void Seed_WhenThereIsAlreadySomeData_MustReturnFalse()
        {
            // Arrange
            testOutputHelper.WriteLine($"Running test method: {GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");

            using (var todoDbContext = GetTodoDbContextMock())
            {
                todoDbContext.TodoItems.Add(new TodoItem());
                todoDbContext.SaveChanges();

                var seedingData = Enumerable.Empty<TodoItem>();
                var logger = new Mock<ILogger<DatabaseSeeder>>().Object;
                var databaseSeeder = new DatabaseSeeder(logger);

                // Act 
                var hasSeededSucceeded = databaseSeeder.Seed(todoDbContext, seedingData);

                // Assert
                hasSeededSucceeded.Should().BeFalse();
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
