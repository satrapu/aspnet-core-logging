using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Todo.Persistence;
using Xunit;

namespace Todo.Services.UnitTests
{
    /// <summary>
    /// Contains unit tests targeting <see cref="TodoService"/> class.
    /// </summary>
    public class TodoServiceTests
    {
        /// <summary>
        /// Tests <see cref="TodoService"/> constructor.
        /// </summary>
        [Fact]
        public void Constructor_WithValidArguments_MustSucceed()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString("N");
            var options = new DbContextOptionsBuilder<TodoDbContext>().UseInMemoryDatabase(dbName).Options;
            var todoDbContext = new TodoDbContext(options);
            var logger = new Mock<ILogger<TodoService>>().Object;

            // Act
            // ReSharper disable once AccessToDisposedClosure
            var exception = Record.Exception(() => new TodoService(todoDbContext, logger));

            // Assert
            exception.Should()
                     .BeNull("class was instantiated using valid arguments");
        }

        /// <summary>
        /// Tests <see cref="TodoService"/> constructor.
        /// </summary>
        /// <param name="todoDbContext"></param>
        /// <param name="logger"></param>
        [Theory]
        [ClassData(typeof(ConstructorTestData))]
        public void Constructor_WithInvalidArguments_MustThrowException(TodoDbContext todoDbContext
                                                                      , ILogger<TodoService> logger)
        {
            // Arrange
            const string reason = "class was instantiated using null argument(s)";

            // Act
            var exception = Record.Exception(() => new TodoService(todoDbContext, logger));

            // Assert
            exception.Should()
                     .NotBeNull(reason)
                     .And.BeOfType<ArgumentNullException>(reason);
        }

        /// <summary>
        /// Contains test data to be used by the parameterized unit tests from <see cref="TodoServiceTests"/> class.
        /// </summary>
        private class ConstructorTestData : TheoryData<TodoDbContext, ILogger<TodoService>>
        {
            public ConstructorTestData()
            {
                AddRow(null, null);
                AddRow(null, new Mock<ILogger<TodoService>>().Object);
                AddRow(new TodoDbContext(new DbContextOptions<TodoDbContext>()), null);
            }
        }
    }
}