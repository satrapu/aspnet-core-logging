using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using Todo.Persistence;
using Todo.TestInfrastructure.Persistence;

namespace Todo.Services
{
    /// <summary>
    /// Contains unit tests targeting <see cref="DatabaseSeeder"/> class.
    /// </summary>
    [TestFixture]
    public class DatabaseSeederTests
    {
        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Test]
        public void Seed_UsingNullAsTodoDbContext_MustThrowException()
        {
            var seedingData = Enumerable.Empty<TodoItem>();
            var mockLogger = new Mock<ILogger<DatabaseSeeder>>();
            var databaseSeeder = new DatabaseSeeder(mockLogger.Object);

            try
            {
                databaseSeeder.Seed(null, seedingData);
            }
            catch (Exception expectedException)
            {
                expectedException.Should()
                         .NotBeNull()
                         .And.BeAssignableTo<ArgumentNullException>()
                         .And.Subject.As<ArgumentNullException>()
                         .ParamName.Should()
                         .Be("todoDbContext");
            }
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Test]
        public void Seed_UsingNullAsSeedingData_MustThrowException()
        {
            using (var todoDbContextFactory = new TodoDbContextFactory())
            {
                var mockLogger = new Mock<ILogger<DatabaseSeeder>>();
                var databaseSeeder = new DatabaseSeeder(mockLogger.Object);

                try
                {
                    databaseSeeder.Seed(todoDbContextFactory.TodoDbContext
                                      , null);
                }
                catch (Exception expectedException)
                {
                    expectedException.Should()
                                     .NotBeNull()
                                     .And.BeAssignableTo<ArgumentNullException>()
                                     .And.Subject.As<ArgumentNullException>()
                                     .ParamName.Should()
                                     .Be("seedingData");
                }
            }
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Test]
        public void Seed_UsingValidArguments_MustReturnTrue()
        {
            using (var todoDbContextFactory = new TodoDbContextFactory())
            {
                var seedingData = Enumerable.Empty<TodoItem>();
                var mockLogger = new Mock<ILogger<DatabaseSeeder>>();
                var databaseSeeder = new DatabaseSeeder(mockLogger.Object);

                // Act 
                var hasSeededSucceeded = databaseSeeder.Seed(todoDbContextFactory.TodoDbContext
                                                           , seedingData);

                // Assert
                hasSeededSucceeded.Should().BeTrue();
            }
        }

        /// <summary>
        /// Tests <see cref="DatabaseSeeder.Seed"/> method.
        /// </summary>
        [Test]
        public void Seed_WhenThereIsAlreadySomeData_MustReturnFalse()
        {
            using (var todoDbContextFactory = new TodoDbContextFactory())
            {
                var todoDbContext = todoDbContextFactory.TodoDbContext;
                todoDbContext.TodoItems.Add(new TodoItem());
                todoDbContext.SaveChanges();

                var seedingData = Enumerable.Empty<TodoItem>();
                var mockLogger = new Mock<ILogger<DatabaseSeeder>>();
                var databaseSeeder = new DatabaseSeeder(mockLogger.Object);

                // Act 
                var hasSeededSucceeded = databaseSeeder.Seed(todoDbContext, seedingData);

                // Assert
                hasSeededSucceeded.Should().BeFalse();
            }
        }
    }
}
