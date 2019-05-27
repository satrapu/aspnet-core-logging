using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using System.Security.Claims;
using Todo.Persistence;
using Todo.Services.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Services
{
    /// <summary>
    /// Contains unit tests targeting <see cref="TodoService"/> class.
    /// </summary>
    public class TodoServiceTests: IClassFixture<TodoDbContextFactory>, IDisposable
    {
        private readonly TodoDbContextFactory todoDbContextFactory;
        private readonly XunitLoggerProvider xunitLoggerProvider;
        private readonly ILogger logger;

        public TodoServiceTests(TodoDbContextFactory todoDbContextFactory, ITestOutputHelper testOutputHelper)
        {
            this.todoDbContextFactory = todoDbContextFactory;
            xunitLoggerProvider = new XunitLoggerProvider(testOutputHelper);
            logger = xunitLoggerProvider.CreateLogger<TodoServiceTests>();
        }

        
        public void Dispose()
        {
            xunitLoggerProvider?.Dispose();
        }

        /// <summary>
        /// Tests <see cref="TodoService"/> constructor.
        /// </summary>
        [Fact]
        public void Constructor_WithValidArguments_MustSucceed()
        {
            // Arrange
            logger.LogInformation("Running test method: {TestMethod}"
                                , $"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");

            var todoDbContext = todoDbContextFactory.TodoDbContext; 
            var mockLogger = new Mock<ILogger<TodoService>>();

            // Act
            // ReSharper disable once AccessToDisposedClosure
            var exception = Record.Exception(() => new TodoService(todoDbContext, mockLogger.Object));

            // Assert
            exception.Should()
                     .BeNull("class was instantiated using valid arguments");
        }

        /// <summary>
        /// Tests <see cref="TodoService"/> constructor.
        /// </summary>
        /// <param name="todoDbContext"></param>
        /// <param name="todoServiceLogger"></param>
        [Theory]
        [ClassData(typeof(ConstructorTestData))]
        public void Constructor_WithInvalidArguments_MustThrowException(TodoDbContext todoDbContext
                                                                      , ILogger<TodoService> todoServiceLogger)
        {
            // Arrange
            logger.LogInformation("Running test method: {TestMethod}"
                                , $"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");
            const string reason = "class was instantiated using null argument(s)";

            // Act
            var exception = Record.Exception(() => new TodoService(todoDbContext, todoServiceLogger));

            // Assert
            exception.Should()
                     .NotBeNull(reason)
                     .And.BeAssignableTo<ArgumentNullException>(reason);
        }

        /// <summary>
        /// Tests <see cref="TodoService.GetByQuery"/> method.
        /// </summary>
        [Fact]
        public void GetByQuery_UsingNullAsQuery_MustThrowException()
        {
            // Arrange
            logger.LogInformation("Running test method: {TestMethod}"
                                , $"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");

            var todoDbContext = todoDbContextFactory.TodoDbContext; 
            var mockLogger = new Mock<ILogger<TodoService>>();
            var todoService = new TodoService(todoDbContext, mockLogger.Object);

            // Act
            var exception = Record.Exception(() => todoService.GetByQuery(null));

            // Assert
            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>()
                     .Subject.As<ArgumentNullException>().ParamName.Should().Be("todoItemQuery");
        }

        /// <summary>
        /// Tests <see cref="TodoService.GetByQuery"/> method.
        /// </summary>
        [Fact]
        public void GetByQuery_UsingValidQuery_MustSucceed()
        {
            // Arrange
            logger.LogInformation("Running test method: {TestMethod}"
                                , $"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");

            var todoDbContext = todoDbContextFactory.TodoDbContext; 
            var mockLogger = new Mock<ILogger<TodoService>>();
            var todoService = new TodoService(todoDbContext, mockLogger.Object);

            var claimsPrincipal = new Mock<ClaimsPrincipal>();
            claimsPrincipal.Setup(x => x.FindFirst(It.IsAny<string>()))
                           .Returns(new Claim(ClaimTypes.NameIdentifier, "satrapu"));

            var todoItemQuery = new TodoItemQuery
            {
                User = claimsPrincipal.Object,
                IsComplete = true
            };

            // Act
            var queryResult = todoService.GetByQuery(todoItemQuery);

            // Assert
            queryResult.Should().NotBeNull();
        }

        /// <summary>
        /// Tests <see cref="TodoService.Add"/> method.
        /// </summary>
        [Fact]
        public void Add_UsingNullAsNewTodoItemInfo_MustThrowException()
        {
            // Arrange
            logger.LogInformation("Running test method: {TestMethod}"
                                , $"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");

            var todoDbContext = todoDbContextFactory.TodoDbContext; 
            var mockLogger = new Mock<ILogger<TodoService>>();
            var todoService = new TodoService(todoDbContext, mockLogger.Object);

            // Act
            var exception = Record.Exception(() => todoService.Add(null));

            // Assert
            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>()
                     .Subject.As<ArgumentNullException>().ParamName.Should().Be("newTodoItemInfo");
        }

        /// <summary>
        /// Tests <see cref="TodoService.Delete"/> method.
        /// </summary>
        [Fact]
        public void Delete_UsingNullAsDeleteTodoItemInfo_MustThrowException()
        {
            // Arrange
            logger.LogInformation("Running test method: {TestMethod}"
                                , $"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");

            var todoDbContext = todoDbContextFactory.TodoDbContext; 
            var mockLogger = new Mock<ILogger<TodoService>>();
            var todoService = new TodoService(todoDbContext, mockLogger.Object);

            // Act
            var exception = Record.Exception(() => todoService.Delete(null));

            // Assert
            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>()
                     .Subject.As<ArgumentNullException>().ParamName.Should().Be("deleteTodoItemInfo");
        }

        /// <summary>
        /// Tests <see cref="TodoService.Delete"/> method.
        /// </summary>
        [Fact]
        public void Delete_UsingNonExistentId_MustThrowException()
        {
            // Arrange
            logger.LogInformation("Running test method: {TestMethod}"
                                , $"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");

            var todoDbContext = todoDbContextFactory.TodoDbContext; 
            var mockLogger = new Mock<ILogger<TodoService>>();
            var todoService = new TodoService(todoDbContext, mockLogger.Object);

            var deleteTodoItemInfo = new DeleteTodoItemInfo
            {
                Id = long.MaxValue
            };

            // Act
            var exception = Record.Exception(() => todoService.Delete(deleteTodoItemInfo));

            // Assert
            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentException>()
                     .Subject.As<ArgumentException>().ParamName.Should().Be("deleteTodoItemInfo");
        }

        /// <summary>
        /// Tests <see cref="TodoService.Update"/> method.
        /// </summary>
        [Fact]
        public void Update_UsingNullAsUpdateTodoItemInfo_MustThrowException()
        {
            // Arrange
            logger.LogInformation("Running test method: {TestMethod}"
                                , $"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}");

            var todoDbContext = todoDbContextFactory.TodoDbContext; 
            var mockLogger = new Mock<ILogger<TodoService>>();
            var todoService = new TodoService(todoDbContext, mockLogger.Object);

            // Act
            var exception = Record.Exception(() => todoService.Update(null));

            // Assert
            exception.Should()
                     .NotBeNull()
                     .And.BeAssignableTo<ArgumentNullException>()
                     .Subject.As<ArgumentNullException>().ParamName.Should().Be("updateTodoItemInfo");
        }

        /// <summary>
        /// Contains test data to be used by the parameterized unit tests from <see cref="TodoServiceTests"/> class.
        /// </summary>
        private class ConstructorTestData : TheoryData<TodoDbContext, ILogger<TodoService>>
        {
            #pragma warning disable S1144 // Unused private types or members should be removed
            public ConstructorTestData()
            {
                var dbContextOptions = new Mock<DbContextOptions<TodoDbContext>>();
                dbContextOptions.SetupGet(x => x.ContextType)
                                .Returns(typeof(DbContext));

                AddRow(null, null);
                AddRow(null, new Mock<ILogger<TodoService>>().Object);
                AddRow(new TodoDbContext(dbContextOptions.Object), null);
            }
            #pragma warning restore S1144 // Unused private types or members should be removed
        }
    }
}