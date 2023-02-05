namespace Todo.Services.TodoItemManagement
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

    /// <summary>
    /// Contains unit tests targeting <see cref="TodoItemService"/> class.
    /// </summary>
    [TestFixture]
    public class TodoItemServiceTests
    {
        private DbContextOptions<TodoDbContext> DummyOptions { get; } = new DbContextOptionsBuilder<TodoDbContext>().Options;

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
                .Should().Throw<ArgumentNullException>($"must not create {nameof(TodoItemService)} with a null {nameof(TodoDbContext)}")
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
                .Should().Throw<ArgumentNullException>($"must not create {nameof(TodoItemService)} with a null {nameof(ILogger<TodoItemService>)}")
                .And.ParamName.Should().Be(nameof(logger), "the logger is null");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.GetByQueryAsync"/> method.
        /// </summary>
        [Test]
        public async Task GetByQueryAsync_UsingNullAsQuery_MustThrowException()
        {
            // Arrange
            DbContextMock<TodoDbContext> mockTodoDbContext = new(DummyOptions);
            Mock<ILogger<TodoItemService>> mockLogger = new();
            TodoItemService todoItemService = new(mockTodoDbContext.Object, mockLogger.Object);
            TodoItemQuery todoItemQuery = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task<IList<TodoItemInfo>>> getByQueryAsyncCall = async () => await todoItemService.GetByQueryAsync(todoItemQuery);

            // Assert
            await getByQueryAsyncCall.Should().ThrowExactlyAsync<ArgumentNullException>("service cannot fetch data using a null query");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.GetByQueryAsync"/> method.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(GetTodoItemQuery))]
        public async Task GetByQueryAsync_UsingValidQuery_MustSucceed(TodoItemQuery todoItemQuery)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TodoItemService>>();

            var owner = new Mock<IPrincipal>();
            owner.SetupGet(x => x.Identity).Returns(new GenericIdentity("test"));

            todoItemQuery.Owner = owner.Object;

            DbContextOptionsBuilder<TodoDbContext> dbContextOptionsBuilder = GetInMemoryDbContextOptionsBuilder();
            TodoItemService todoItemService = new(new TodoDbContext(dbContextOptionsBuilder.Options), mockLogger.Object);

            // Act
            Func<Task<IList<TodoItemInfo>>> getByQueryAsyncCall = async () => await todoItemService.GetByQueryAsync(todoItemQuery);

            // Assert
            await getByQueryAsyncCall.Should().NotThrowAsync("query is valid");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.AddAsync"/> method.
        /// </summary>
        [Test]
        public async Task AddAsync_UsingNullAsNewTodoItemInfo_MustThrowException()
        {
            // Arrange
            DbContextMock<TodoDbContext> mockTodoDbContext = new(DummyOptions);
            Mock<ILogger<TodoItemService>> mockLogger = new();
            TodoItemService todoItemService = new(mockTodoDbContext.Object, mockLogger.Object);
            NewTodoItemInfo newTodoItemInfo = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task<long>> addAsyncCall = async () => await todoItemService.AddAsync(newTodoItemInfo);

            // Assert
            await addAsyncCall.Should().ThrowExactlyAsync<ArgumentNullException>("service cannot add data using a null item");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.UpdateAsync"/> method.
        /// </summary>
        [Test]
        public async Task UpdateAsync_UsingNullAsUpdateTodoItemInfo_MustThrowException()
        {
            // Arrange
            DbContextMock<TodoDbContext> mockTodoDbContext = new(DummyOptions);
            Mock<ILogger<TodoItemService>> mockLogger = new();
            TodoItemService todoItemService = new(mockTodoDbContext.Object, mockLogger.Object);
            UpdateTodoItemInfo updateTodoItemInfo = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task> updateAsyncCall = async () => await todoItemService.UpdateAsync(updateTodoItemInfo);

            // Assert
            await updateAsyncCall.Should().ThrowExactlyAsync<ArgumentNullException>("service cannot update data using a null item");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.UpdateAsync"/> method.
        /// </summary>
        [Test]
        public async Task UpdateAsync_UsingNonexistentEntityKey_MustThrowException()
        {
            // Arrange
            Mock<ILogger<TodoItemService>> mockLogger = new();

            Mock<IPrincipal> owner = new();
            owner.SetupGet(x => x.Identity).Returns(new GenericIdentity("test"));

            UpdateTodoItemInfo updateTodoItemInfo = new()
            {
                Id = long.MaxValue,
                Name = "test",
                IsComplete = true,
                Owner = owner.Object
            };

            DbContextOptionsBuilder<TodoDbContext> dbContextOptionsBuilder = GetInMemoryDbContextOptionsBuilder();
            TodoItemService todoItemService = new(new(dbContextOptionsBuilder.Options), mockLogger.Object);

            // Act
            Func<Task> updateAsyncCall = async () => await todoItemService.UpdateAsync(updateTodoItemInfo);

            // Assert
            await updateAsyncCall.Should().ThrowExactlyAsync<EntityNotFoundException>("service cannot update data using nonexistent entity key");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.DeleteAsync"/> method.
        /// </summary>
        [Test]
        public async Task DeleteAsync_UsingNullAsDeleteTodoItemInfo_MustThrowException()
        {
            // Arrange
            DbContextMock<TodoDbContext> mockTodoDbContext = new(DummyOptions);
            Mock<ILogger<TodoItemService>> mockLogger = new();
            TodoItemService todoItemService = new(mockTodoDbContext.Object, mockLogger.Object);
            DeleteTodoItemInfo deleteTodoItemInfo = null;

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            Func<Task> deleteAsyncCall = async () => await todoItemService.DeleteAsync(deleteTodoItemInfo);

            // Assert
            await deleteAsyncCall
                .Should().ThrowExactlyAsync<ArgumentNullException>("service cannot delete data using a null item")
                .WithParameterName(nameof(deleteTodoItemInfo), "the item is null");
        }

        /// <summary>
        /// Tests <see cref="TodoItemService.DeleteAsync"/> method.
        /// </summary>
        [Test]
        public async Task DeleteAsync_UsingNonexistentEntityKey_MustThrowException()
        {
            // Arrange
            Mock<ILogger<TodoItemService>> mockLogger = new();

            Mock<IPrincipal> owner = new();
            owner.SetupGet(x => x.Identity).Returns(new GenericIdentity("test"));

            DeleteTodoItemInfo deleteTodoItemInfo = new()
            {
                Id = long.MaxValue,
                Owner = owner.Object
            };

            DbContextOptionsBuilder<TodoDbContext> dbContextOptionsBuilder = GetInMemoryDbContextOptionsBuilder();
            TodoItemService todoItemService = new(new(dbContextOptionsBuilder.Options), mockLogger.Object);

            // Act
            Func<Task> deleteAsyncCall = async () => await todoItemService.DeleteAsync(deleteTodoItemInfo);

            // Assert
            await deleteAsyncCall.Should().ThrowExactlyAsync<EntityNotFoundException>("service cannot delete data using nonexistent entity key");
        }

        private static DbContextOptionsBuilder<TodoDbContext> GetInMemoryDbContextOptionsBuilder()
        {
            string databaseName = $"db--{Guid.NewGuid():N}";

            return
                new DbContextOptionsBuilder<TodoDbContext>()
                    .UseInMemoryDatabase(databaseName)
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging();
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
                        SortBy = null,
                        IsSortAscending = null
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
                        SortBy = "PropertyWhichDoesNotExist",
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
                        SortBy = "PropertyWhichDoesNotExist",
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
                        SortBy = "PropertyWhichDoesNotExist",
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
                        SortBy = nameof(TodoItem.Id),
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
                        SortBy = nameof(TodoItem.CreatedOn),
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
                        SortBy = nameof(TodoItem.LastUpdatedOn),
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
                        SortBy = nameof(TodoItem.Name),
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
