namespace Todo.Persistence
{
    using System;
    using System.Threading.Tasks;

    using Commons.Constants;

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
            DbContextOptions<TodoDbContext> dbContextOptions = GetDbContextOptions(databaseName: "db-migrations-have-run-successfully");
            await using TodoDbContext todoDbContext = new(dbContextOptions);
            bool isMigrationSuccessful;

            try
            {
                await todoDbContext.Database.EnsureDeletedAsync();
                IMigrator databaseMigrator = todoDbContext.GetInfrastructure().GetRequiredService<IMigrator>();

                // Act
                await databaseMigrator.MigrateAsync();
                // Revert migrations by using a special migration identifier.
                // See more here: https://learn.microsoft.com/en-us/ef/core/cli/dotnet#dotnet-ef-database-update.
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
            DbContextOptions<TodoDbContext> dbContextOptions = GetDbContextOptions(databaseName: "use-concurrent-transactions");
            await using TodoDbContext firstTodoDbContext = new(dbContextOptions);
            IMigrator databaseMigrator = firstTodoDbContext.GetInfrastructure().GetRequiredService<IMigrator>();
            await databaseMigrator.MigrateAsync();

            try
            {
                const string name = "ConcurrentlyAccessedTodoItem";

                TodoItem todoItem = new(name, "it")
                {
                    IsComplete = false
                };

                await firstTodoDbContext.TodoItems.AddAsync(todoItem);
                await firstTodoDbContext.SaveChangesAsync();

                await using IDbContextTransaction firstTransaction = await firstTodoDbContext.Database.BeginTransactionAsync();

                TodoItem todoItemFromFirstTransaction = await firstTodoDbContext.TodoItems.FirstAsync(x => x.Name == name);
                todoItemFromFirstTransaction.IsComplete = true;
                todoItemFromFirstTransaction.LastUpdatedBy = Guid.NewGuid().ToString("N");
                todoItemFromFirstTransaction.LastUpdatedOn = DateTime.UtcNow;

                await using TodoDbContext secondTodoDbContext = new(dbContextOptions);
                await using IDbContextTransaction secondTransaction = await secondTodoDbContext.Database.BeginTransactionAsync();

                TodoItem todoItemFromSecondTransaction = await secondTodoDbContext.TodoItems.FirstAsync(x => x.Name == name);
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
                await saveChangesAsyncCall
                    .Should()
                    .ThrowExactlyAsync<DbUpdateConcurrencyException>("2 transactions were concurrently modifying the same entity");
            }
            finally
            {
                await firstTodoDbContext.Database.EnsureDeletedAsync();
            }
        }

        private static DbContextOptions<TodoDbContext> GetDbContextOptions(string databaseName)
        {
            string connectionString =
                new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{EnvironmentNames.IntegrationTests}.json", optional: false, reloadOnChange: false)
                    .AddEnvironmentVariables(prefix: EnvironmentVariables.Prefix)
                    .Build()
                    .GetConnectionString(ConnectionStrings.UsedByIntegrationTests);

            NpgsqlConnectionStringBuilder dbConnectionStringBuilder = new(connectionString)
            {
                Database = databaseName
            };

            DbContextOptions<TodoDbContext> dbContextOptions =
                new DbContextOptionsBuilder<TodoDbContext>()
                    .UseNpgsql(dbConnectionStringBuilder.ConnectionString)
                    .Options;

            return dbContextOptions;
        }
    }
}
