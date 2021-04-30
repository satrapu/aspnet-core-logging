namespace Todo.Persistence
{
    using System;
    using System.Threading.Tasks;

    using Entities;

    using FluentAssertions;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Migrations;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Npgsql;

    using NUnit.Framework;

    /// <summary>
    /// Contains integration tests targeting <see cref="TodoDbContext"/> class.
    /// </summary>
    [TestFixture]
    public class TodoDbContextTests
    {
        private const string BeforeFirstDatabaseMigration = "0";

        /// <summary>
        /// Checks whether EF Core migrations can be run in both directions, up and down.
        /// </summary>
        [Test]
        public async Task GivenTheCurrentMigrations_WhenRunningThemInBothDirections_DatabaseIsCorrectlyMigrated()
        {
            // Arrange
            string databaseName =
                $"it--{nameof(GivenTheCurrentMigrations_WhenRunningThemInBothDirections_DatabaseIsCorrectlyMigrated)}";

            DbContextOptions<TodoDbContext> dbContextOptions = GetDbContextOptions(databaseName);
            await using TodoDbContext todoDbContext = new TodoDbContext(dbContextOptions);
            bool isMigrationSuccessful;

            try
            {
                await todoDbContext.Database.EnsureDeletedAsync();
                IMigrator databaseMigrator = todoDbContext.GetInfrastructure().GetRequiredService<IMigrator>();

                // Act
                await databaseMigrator.MigrateAsync();
                // Revert migrations by using a special migration identifier.
                // See more here: https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet#dotnet-ef-database-update.
                await databaseMigrator.MigrateAsync(BeforeFirstDatabaseMigration);
                await databaseMigrator.MigrateAsync();
                isMigrationSuccessful = true;
            }
            catch
            {
                isMigrationSuccessful = false;
            }
            finally
            {
                await todoDbContext.Database.EnsureDeletedAsync();
            }

            // Assert
            isMigrationSuccessful.Should().BeTrue("migrations should work in both directions, up and down");
        }

        /// <summary>
        /// Ensures that trying to modify same entity from within 2 concurrent transactions fails.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task SaveChanges_WhenModifyingSameEntityUsingTwoConcurrentTransactions_ThrowsException()
        {
            // Arrange
            string databaseName =
                $"it--{nameof(SaveChanges_WhenModifyingSameEntityUsingTwoConcurrentTransactions_ThrowsException)}";

            DbContextOptions<TodoDbContext> dbContextOptions = GetDbContextOptions(databaseName);

            await using TodoDbContext firstTodoDbContext = new TodoDbContext(dbContextOptions);
            IMigrator databaseMigrator = firstTodoDbContext.GetInfrastructure().GetRequiredService<IMigrator>();
            await databaseMigrator.MigrateAsync();

            try
            {
                string name = "ConcurrentlyAccessedTodoItem";

                TodoItem todoItem = new TodoItem(name, "it")
                {
                    IsComplete = false,
                };

                await firstTodoDbContext.TodoItems.AddAsync(todoItem);
                await firstTodoDbContext.SaveChangesAsync();

                await using IDbContextTransaction firstTransaction =
                    await firstTodoDbContext.Database.BeginTransactionAsync();

                TodoItem todoItemFromFirstTransaction =
                    await firstTodoDbContext.TodoItems.FirstAsync(x => x.Name == name);

                todoItemFromFirstTransaction.IsComplete = true;
                todoItemFromFirstTransaction.LastUpdatedBy = Guid.NewGuid().ToString("N");
                todoItemFromFirstTransaction.LastUpdatedOn = DateTime.UtcNow;

                await using TodoDbContext secondTodoDbContext = new TodoDbContext(dbContextOptions);

                await using IDbContextTransaction secondTransaction =
                    await secondTodoDbContext.Database.BeginTransactionAsync();

                TodoItem todoItemFromSecondTransaction =
                    await secondTodoDbContext.TodoItems.FirstAsync(x => x.Name == name);

                todoItemFromSecondTransaction.IsComplete = false;
                todoItemFromSecondTransaction.LastUpdatedBy = Guid.NewGuid().ToString("N");
                todoItemFromSecondTransaction.LastUpdatedOn = DateTime.UtcNow;

                await firstTodoDbContext.SaveChangesAsync();
                await firstTransaction.CommitAsync();

                // Act
                Func<Task> saveChangesAsyncCall = async () =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    await secondTodoDbContext.SaveChangesAsync();
                    await secondTransaction.CommitAsync();
                    // ReSharper restore AccessToDisposedClosure
                };

                // Assert
                saveChangesAsyncCall.Should()
                    .ThrowExactly<DbUpdateConcurrencyException>(
                        "2 transactions were concurrently modifying the same entity");
            }
            finally
            {
                await firstTodoDbContext.Database.EnsureDeletedAsync();
            }
        }

        private static DbContextOptions<TodoDbContext> GetDbContextOptions(string databaseName)
        {
            var configurationBuilder = new ConfigurationBuilder();

            IConfigurationRoot testConfiguration = configurationBuilder.AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.IntegrationTests.json", false)
                .AddEnvironmentVariables()
                .Build();

            // ReSharper disable once SettingNotFoundInConfiguration
            var testConnectionString = testConfiguration.GetConnectionString("TodoForIntegrationTests");

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(testConnectionString)
            {
                Database = $"it--{databaseName}"
            };

            DbContextOptions<TodoDbContext> dbContextOptions = new DbContextOptionsBuilder<TodoDbContext>()
                .UseNpgsql(connectionStringBuilder.ConnectionString).Options;

            return dbContextOptions;
        }
    }
}
