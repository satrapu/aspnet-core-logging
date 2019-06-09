using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using Todo.Persistence;

namespace Todo.Services
{
    /// <summary>
    /// Contains unit tests targeting <see cref="TodoService"/> class.
    /// </summary>
    [TestFixture]
    public class TodoServiceTests
    {
        private DbContextOptions<TodoDbContext> DummyOptions { get; } = new DbContextOptionsBuilder<TodoDbContext>().Options;

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
        /// Tests <see cref="TodoService.GetByQuery"/> method.
        /// </summary>
        [Test]
        public void GetByQuery_UsingNullAsQuery_MustThrowException()
        {
            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            var mockLogger = new Mock<ILogger<TodoService>>();
            var todoService = new TodoService(mockTodoDbContext.Object, mockLogger.Object);
            TodoItemQuery todoItemQuery = null;

            try
            {
                // ReSharper disable once ExpressionIsAlwaysNull
                todoService.GetByQuery(todoItemQuery);
                Assert.Fail($"Expected to not be able to call method {nameof(todoService.GetByQuery)} using null as argument");
            }
            catch (Exception expectedException)
            {
                expectedException.Should()
                                 .NotBeNull()
                                 .And.BeAssignableTo<ArgumentNullException>()
                                 .Subject.As<ArgumentNullException>()
                                 .ParamName.Should().Be("todoItemQuery");
            }
        }
    }
}