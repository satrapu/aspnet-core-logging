using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Todo.Persistence;

namespace Todo.Services
{
    /// <summary>
    /// Contains unit tests targeting <see cref="TodoService"/> class.
    /// </summary>
    [TestFixture]
    public class TodoServiceTests
    {
        private DbContextOptions<TodoDbContext> DummyOptions { get; } =
            new DbContextOptionsBuilder<TodoDbContext>().Options;

        /// <summary>
        /// Tests <see cref="TodoService"/> constructor.
        /// </summary>
        [Test]
        public void Constructor_WithValidArguments_MustSucceed()
        {
            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            var mockLogger = new Mock<ILogger<TodoService>>();

            try
            {
                var todoService = new TodoService(mockTodoDbContext.Object, mockLogger.Object);
                todoService.Should().NotBeNull();
            }
            catch
            {
                Assert.Fail($"Expected to create an instance of the {nameof(TodoService)} class using valid arguments");
            }
        }

        /// <summary>
        /// Tests <see cref="TodoService"/> constructor.
        /// </summary>
        [Test]
        public void Constructor_WithNullDbContext_ThrowsException()
        {
            TodoDbContext todoDbContext = null;
            var mockLogger = new Mock<ILogger<TodoService>>();

            // ReSharper disable once ExpressionIsAlwaysNull
            // ReSharper disable once ObjectCreationAsStatement
            Action createService = () => new TodoService(todoDbContext, mockLogger.Object);
            createService.Should()
                .Throw<ArgumentNullException>(
                    $"must not create {nameof(TodoService)} with a null {nameof(TodoDbContext)}")
                .And.ParamName.Should().Be(nameof(todoDbContext), "the EF Core context is null");
        }

        /// <summary>
        /// Tests <see cref="TodoService"/> constructor.
        /// </summary>
        [Test]
        public void Constructor_WithNullLogger_ThrowsException()
        {
            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            ILogger<TodoService> logger = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            // ReSharper disable once ObjectCreationAsStatement
            Action createService = () => new TodoService(mockTodoDbContext.Object, logger);
            createService.Should()
                .Throw<ArgumentNullException>(
                    $"must not create {nameof(TodoService)} with a null {nameof(ILogger<TodoService>)}")
                .And.ParamName.Should().Be(nameof(logger), "the logger is null");
        }

        /// <summary>
        /// Tests <see cref="TodoService.GetByQueryAsync"/> method.
        /// </summary>
        [Test]
        public void GetByQueryAsync_UsingNullAsQuery_MustThrowException()
        {
            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            var mockLogger = new Mock<ILogger<TodoService>>();
            var todoService = new TodoService(mockTodoDbContext.Object, mockLogger.Object);
            TodoItemQuery todoItemQuery = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task<IList<TodoItemInfo>>> getByQueryAsync = async () =>
                await todoService.GetByQueryAsync(todoItemQuery).ConfigureAwait(false);
            getByQueryAsync.Should().Throw<ArgumentNullException>("service cannot fetch data using a null query")
                .And.ParamName.Should().Be("instance", "the query is null");
        }
    }
}