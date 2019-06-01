using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using Todo.TestInfrastructure.Persistence;

namespace Todo.Services
{
    /// <summary>
    /// Contains unit tests targeting <see cref="TodoService"/> class.
    /// </summary>
    [TestFixture]
    public class TodoServiceTests
    {
        
        /// <summary>
        /// Tests <see cref="TodoService"/> constructor.
        /// </summary>
        [Test]
        public void Constructor_WithValidArguments_MustSucceed()
        {
            using (var todoDbContextFactory = new TodoDbContextFactory())
            using(var todoDbContext = todoDbContextFactory.TodoDbContext)
            {
                var mockLogger = new Mock<ILogger<TodoService>>();

                try
                {
                    var todoService = new TodoService(todoDbContext, mockLogger.Object);
                    todoService.Should().NotBeNull();
                }
                catch
                {
                    Assert.Fail("Expected to create an instance of the "
                              + $"{nameof(TodoService)} class using valid arguments");
                }
            }
        }

        /// <summary>
        /// Tests <see cref="TodoService.GetByQuery"/> method.
        /// </summary>
        [Test]
        public void GetByQuery_UsingNullAsQuery_MustThrowException()
        {
            using(var todoDbContextFactory = new TodoDbContextFactory())
            using(var todoDbContext = todoDbContextFactory.TodoDbContext)
            {
                var mockLogger = new Mock<ILogger<TodoService>>();
                var todoService = new TodoService(todoDbContext, mockLogger.Object);

                try
                {
                    todoService.GetByQuery(null);
                    Assert.Fail("Expected to not be able to call method "
                              + $"{nameof(todoService.GetByQuery)} using null as argument");
                }
                catch (Exception expectedException)
                {
                    expectedException.Should()
                             .NotBeNull()
                             .And.BeAssignableTo<ArgumentNullException>()
                             .Subject.As<ArgumentNullException>()
                             .ParamName.Should()
                             .Be("todoItemQuery");
                }
            }
        }
    }
}