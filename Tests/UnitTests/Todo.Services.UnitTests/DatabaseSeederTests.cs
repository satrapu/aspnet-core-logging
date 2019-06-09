using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Todo.Persistence;

namespace Todo.Services
{
    /// <summary>
    /// Contains unit tests targeting <see cref="DatabaseSeeder"/> class.
    /// </summary>
    [TestFixture]
    public class DatabaseSeederTests
    {
        private DbContextOptions<TodoDbContext> DummyOptions { get; } = new DbContextOptionsBuilder<TodoDbContext>().Options;

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Test]
        public void Seed_UsingNullAsTodoDbContext_MustThrowException()
        {
            // Arrange
            var seedingData = Enumerable.Empty<TodoItem>();
            var mockLogger = new Mock<ILogger<DatabaseSeeder>>();
            var databaseSeeder = new DatabaseSeeder(mockLogger.Object);
            TodoDbContext todoDbContext = null;

            try
            {
                // Act
                // ReSharper disable once ExpressionIsAlwaysNull
                databaseSeeder.Seed(todoDbContext, seedingData);
            }
            catch (Exception expectedException)
            {
                // Assert
                expectedException.Should()
                                 .NotBeNull()
                                 .And.BeAssignableTo<ArgumentNullException>()
                                 .And.Subject.As<ArgumentNullException>().ParamName.Should().Be(nameof(todoDbContext));
            }
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Test]
        public void Seed_UsingNullAsSeedingData_MustThrowException()
        {
            // Arrange
            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            var mockLogger = new Mock<ILogger<DatabaseSeeder>>();
            var databaseSeeder = new DatabaseSeeder(mockLogger.Object);
            IEnumerable<TodoItem> seedingData = null;

            try
            {
                // Act
                // ReSharper disable once ExpressionIsAlwaysNull
                databaseSeeder.Seed(mockTodoDbContext.Object, seedingData);
            }
            catch (Exception expectedException)
            {
                // Assert
                expectedException.Should()
                                 .NotBeNull()
                                 .And.BeAssignableTo<ArgumentNullException>()
                                 .And.Subject.As<ArgumentNullException>().ParamName.Should().Be(nameof(seedingData));
            }
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Test]
        public void Seed_WhenInvokingDbContextFails_MustThrowException()
        {
            // Arrange
            var hardCodedException = new InvalidOperationException("Cannot access the underlying database");

            var mockTodoDbContext = new DbContextMock<TodoDbContext>(DummyOptions);
            mockTodoDbContext.SetupGet(x => x.Database)
                             .Throws(hardCodedException);

            var mockLogger = new Mock<ILogger<DatabaseSeeder>>();
            var databaseSeeder = new DatabaseSeeder(mockLogger.Object);
            var seedingData = Enumerable.Empty<TodoItem>();

            try
            {
                // Act
                // ReSharper disable once ExpressionIsAlwaysNull
                databaseSeeder.Seed(mockTodoDbContext.Object, seedingData);
            }
            catch (Exception expectedException)
            {
                // Assert
                expectedException.Should()
                                 .NotBeNull()
                                 .And.BeAssignableTo<InvalidOperationException>()
                                 .And.Subject.As<InvalidOperationException>().Message.Should().Be(hardCodedException.Message);
            }
        }
    }
}
