namespace Todo.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using EntityFrameworkCoreMock;

    using FluentAssertions;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    using Persistence;
    using Persistence.Entities;

    using TodoItemLifecycleManagement;

    /// <summary>
    /// Contains unit tests targeting <see cref="TodoItemService"/> class.
    /// </summary>
    [TestFixture]
    public class TodoItemServiceTests
    {
        private DbContextOptions<TodoDbContext> DummyOptions { get; } =
            new DbContextOptionsBuilder<TodoDbContext>().Options;

        /// <summary>
        /// Tests <see cref="TodoItemService"/> constructor.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1806:Do not ignore method results",
            Justification = "Newly created instance is discarded for testing purposes")]
        public void Constructor_WithValidArguments_MustSucceed()
        {
            // Arrange
            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            var mockLogger = new Mock<ILogger<TodoItemService>>();

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action createTodoItemServiceAction = () => new TodoItemService(mockTodoDbContext.Object, mockLogger.Object);

            // Assert
            createTodoItemServiceAction.Should().NotThrow("all constructor parameters were correctly provided");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService"/> constructor.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1806:Do not ignore method results",
            Justification = "Newly created instance is discarded for testing purposes")]
        public void Constructor_WithNullDbContext_ThrowsException()
        {
            // Arrange
            TodoDbContext todoDbContext = null;
            var mockLogger = new Mock<ILogger<TodoItemService>>();

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            // ReSharper disable once ObjectCreationAsStatement
            Action createTodoItemServiceAction = () => new TodoItemService(todoDbContext, mockLogger.Object);

            // Assert
            createTodoItemServiceAction
                .Should().Throw<ArgumentNullException>(
                    $"must not create {nameof(TodoItemService)} with a null {nameof(TodoDbContext)}")
                .And.ParamName.Should().Be(nameof(todoDbContext), "the EF Core context is null");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService"/> constructor.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1806:Do not ignore method results",
            Justification = "Newly created instance is discarded for testing purposes")]
        public void Constructor_WithNullLogger_ThrowsException()
        {
            // Arrange
            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            ILogger<TodoItemService> logger = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            // ReSharper disable once ObjectCreationAsStatement
            Action createTodoItemServiceAction = () => new TodoItemService(mockTodoDbContext.Object, logger);

            // Assert
            createTodoItemServiceAction
                .Should().Throw<ArgumentNullException>(
                    $"must not create {nameof(TodoItemService)} with a null {nameof(ILogger<TodoItemService>)}")
                .And.ParamName.Should().Be(nameof(logger), "the logger is null");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.GetByQueryAsync"/> method.
        /// </summary>
        [Test]
        public void GetByQueryAsync_UsingNullAsQuery_MustThrowException()
        {
            // Arrange
            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            var mockLogger = new Mock<ILogger<TodoItemService>>();
            var todoItemService = new TodoItemService(mockTodoDbContext.Object, mockLogger.Object);
            TodoItemQuery todoItemQuery = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task<IList<TodoItemInfo>>> getByQueryAsyncCall =
                async () => await todoItemService.GetByQueryAsync(todoItemQuery);

            // Assert
            getByQueryAsyncCall.Should().Throw<ArgumentNullException>("service cannot fetch data using a null query");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.GetByQueryAsync"/> method.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(GetTodoItemQuery))]
        public void GetByQueryAsync_UsingValidQuery_MustSucceed(TodoItemQuery todoItemQuery)
        {
            // Arrange
            var inMemoryDatabase =
                new DbContextOptionsBuilder<TodoDbContext>()
                    .UseInMemoryDatabase($"db--{Guid.NewGuid():N}")
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging();

            var mockLogger = new Mock<ILogger<TodoItemService>>();

            var owner = new Mock<IPrincipal>();
            owner.SetupGet(x => x.Identity).Returns(new GenericIdentity("test"));

            todoItemQuery.Owner = owner.Object;

            var todoItemService = new TodoItemService(new TodoDbContext(inMemoryDatabase.Options), mockLogger.Object);

            // Act
            Func<Task<IList<TodoItemInfo>>> getByQueryAsyncCall =
                async () => await todoItemService.GetByQueryAsync(todoItemQuery);

            // Assert
            getByQueryAsyncCall.Should().NotThrow("query is valid");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.AddAsync"/> method.
        /// </summary>
        [Test]
        public void AddAsync_UsingNullAsNewTodoItemInfo_MustThrowException()
        {
            // Arrange
            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            var mockLogger = new Mock<ILogger<TodoItemService>>();
            var todoItemService = new TodoItemService(mockTodoDbContext.Object, mockLogger.Object);
            NewTodoItemInfo newTodoItemInfo = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task<long>> addAsyncCall = async () => await todoItemService.AddAsync(newTodoItemInfo);

            // Assert
            addAsyncCall.Should().Throw<ArgumentNullException>("service cannot add data using a null item");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.UpdateAsync"/> method.
        /// </summary>
        [Test]
        public void UpdateAsync_UsingNullAsUpdateTodoItemInfo_MustThrowException()
        {
            // Arrange
            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            var mockLogger = new Mock<ILogger<TodoItemService>>();
            var todoItemService = new TodoItemService(mockTodoDbContext.Object, mockLogger.Object);
            UpdateTodoItemInfo updateTodoItemInfo = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task> updateAsyncCall = async () => await todoItemService.UpdateAsync(updateTodoItemInfo);

            // Assert
            updateAsyncCall.Should().Throw<ArgumentNullException>("service cannot update data using a null item");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.UpdateAsync"/> method.
        /// </summary>
        [Test]
        public void UpdateAsync_UsingNonexistentEntityKey_MustThrowException()
        {
            // Arrange
            var inMemoryDatabase =
                new DbContextOptionsBuilder<TodoDbContext>()
                    .UseInMemoryDatabase($"db--{Guid.NewGuid():N}")
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging();

            var mockLogger = new Mock<ILogger<TodoItemService>>();

            var owner = new Mock<IPrincipal>();
            owner.SetupGet(x => x.Identity).Returns(new GenericIdentity("test"));

            var updateTodoItemInfo = new UpdateTodoItemInfo
            {
                Id = long.MaxValue,
                Name = "test",
                IsComplete = true,
                Owner = owner.Object
            };

            var todoItemService = new TodoItemService(new TodoDbContext(inMemoryDatabase.Options), mockLogger.Object);

            // Act
            Func<Task> updateAsyncCall = async () => await todoItemService.UpdateAsync(updateTodoItemInfo);

            // Assert
            updateAsyncCall.Should().Throw<EntityNotFoundException>(
                "service cannot update data using nonexistent entity key");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.DeleteAsync"/> method.
        /// </summary>
        [Test]
        public void DeleteAsync_UsingNullAsDeleteTodoItemInfo_MustThrowException()
        {
            // Arrange
            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            var mockLogger = new Mock<ILogger<TodoItemService>>();
            var todoItemService = new TodoItemService(mockTodoDbContext.Object, mockLogger.Object);
            DeleteTodoItemInfo deleteTodoItemInfo = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task> deleteAsyncCall = async () => await todoItemService.DeleteAsync(deleteTodoItemInfo);

            // Assert
            deleteAsyncCall
                .Should().Throw<ArgumentNullException>("service cannot delete data using a null item")
                .And.ParamName.Should().Be(nameof(deleteTodoItemInfo), "the item is null");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.DeleteAsync"/> method.
        /// </summary>
        [Test]
        public void DeleteAsync_UsingNonexistentEntityKey_MustThrowException()
        {
            // Arrange
            var inMemoryDatabase =
                new DbContextOptionsBuilder<TodoDbContext>()
                    .UseInMemoryDatabase($"db--{Guid.NewGuid():N}")
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging();

            var mockLogger = new Mock<ILogger<TodoItemService>>();

            var owner = new Mock<IPrincipal>();
            owner.SetupGet(x => x.Identity).Returns(new GenericIdentity("test"));

            var deleteTodoItemInfo = new DeleteTodoItemInfo
            {
                Id = long.MaxValue,
                Owner = owner.Object
            };

            var todoItemService = new TodoItemService(new TodoDbContext(inMemoryDatabase.Options), mockLogger.Object);

            // Act
            Func<Task> deleteAsyncCall = async () => await todoItemService.DeleteAsync(deleteTodoItemInfo);

            // Assert
            deleteAsyncCall.Should().Throw<EntityNotFoundException>(
                "service cannot delete data using nonexistent entity key");
        }

        private static IEnumerable<object[]> GetTodoItemQuery()
        {
            return new List<object[]>
            {
                new object[]
                {
                    new TodoItemQuery
                    {
                        Id = long.MaxValue,
                        IsComplete = true,
                        NamePattern = "%",
                        PageIndex = 1,
                        PageSize = 100,
                        SortBy = nameof(TodoItem.Id),
                        IsSortAscending = true
                    }
                },
                new object[]
                {
                    new TodoItemQuery
                    {
                        Id = long.MaxValue,
                        IsComplete = true,
                        NamePattern = "%",
                        PageIndex = 1,
                        PageSize = 100,
                        SortBy = nameof(TodoItem.CreatedOn),
                        IsSortAscending = false
                    }
                },
                new object[]
                {
                    new TodoItemQuery
                    {
                        Id = long.MaxValue,
                        IsComplete = true,
                        NamePattern = "%",
                        PageIndex = 1,
                        PageSize = 100,
                        SortBy = nameof(TodoItem.LastUpdatedOn),
                        IsSortAscending = true
                    }
                },
                new object[]
                {
                    new TodoItemQuery
                    {
                        Id = long.MaxValue,
                        IsComplete = true,
                        NamePattern = "%",
                        PageIndex = 1,
                        PageSize = 100,
                        SortBy = nameof(TodoItem.Name),
                        IsSortAscending = false
                    }
                }
            };
        }
    }
}
