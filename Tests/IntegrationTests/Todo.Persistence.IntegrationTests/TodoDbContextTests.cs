using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;

namespace Todo.Persistence
{
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
        public void GivenTheCurrentMigrations_WhenApplyingAndRevertingAndApplyingThemAgain_DatabaseIsCorrectlyMigrated()
        {
            // Arrange
            DbContextOptions<TodoDbContext> dbContextOptions = GetDbContextOptions();

            using (TodoDbContext todoDbContext = new TodoDbContext(dbContextOptions))
            {
                bool isMigrationSuccessful;

                try
                {
                    todoDbContext.Database.EnsureDeleted();
                    IMigrator databaseMigrator = todoDbContext.GetInfrastructure().GetRequiredService<IMigrator>();

                    // Act
                    databaseMigrator.Migrate();
                    // Revert migrations by using a special migration identifier.
                    // See more here: https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet#dotnet-ef-database-update.
                    databaseMigrator.Migrate(BeforeFirstDatabaseMigration);
                    databaseMigrator.Migrate();
                    isMigrationSuccessful = true;
                }
                catch
                {
                    isMigrationSuccessful = false;
                }

                // Assert
                isMigrationSuccessful.Should().BeTrue("migrations should work in both directions, up and down");
            }
        }

        private static DbContextOptions<TodoDbContext> GetDbContextOptions()
        {
            var configurationBuilder = new ConfigurationBuilder();
            IConfigurationRoot testConfiguration = configurationBuilder.AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.IntegrationTests.json", false)
                .AddEnvironmentVariables()
                .Build();
            var testConnectionString = testConfiguration.GetConnectionString("TodoForIntegrationTests");
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(testConnectionString)
            {
                Database = $"it--{MethodBase.GetCurrentMethod().DeclaringType?.Name}"
            };

            DbContextOptions<TodoDbContext> dbContextOptions = new DbContextOptionsBuilder<TodoDbContext>()
                .UseNpgsql(connectionStringBuilder.ConnectionString).Options;

            return dbContextOptions;
        }
    }
}