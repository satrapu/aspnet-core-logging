using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Microsoft.EntityFrameworkCore;
using TodoWebApp.Models;
using TodoWebApp.Services;
using Xunit;

namespace TodoWebApp.UnitTests.Services
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
            var dbContext = GetDbContextForTestingPurposes();
            var logger = new Mock<ILogger<TodoService>>().Object;

            // Act
            var exception = Record.Exception(() => new TodoService(dbContext, logger));

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
        [ClassData(typeof(TodoServiceTestData))]
        public void Constructor_WithNullArguments_MustThrowException(TodoDbContext todoDbContext
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
        /// Creates an instance of the <see cref="TodoDbContext"/> class to be used for testing purposes.
        /// </summary>
        /// <returns></returns>
        private static TodoDbContext GetDbContextForTestingPurposes()
        {
            var inMemoryDatabaseName = $"{nameof(TodoServiceTests)}-{Guid.NewGuid():N}";
            var options = new DbContextOptionsBuilder<TodoDbContext>().UseInMemoryDatabase(inMemoryDatabaseName).Options;
            var result = new TodoDbContext(options);
            return result;
        }
        
        /// <summary>
        /// Contains test data to be used by the parameterized unit tests from <see cref="TodoServiceTests"/> class.
        /// </summary>
        private class TodoServiceTestData : TheoryData<TodoDbContext, ILogger<TodoService>>
        {
            public TodoServiceTestData()
            {
                AddRow(null, null);
                AddRow(null, new Mock<ILogger<TodoService>>().Object);
                AddRow(GetDbContextForTestingPurposes(), null);
            }
        }
    }
}