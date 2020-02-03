using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Todo.IntegrationTests.Infrastructure;

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
            using (var testWebApplicationFactory =
                new TestWebApplicationFactory(MethodBase.GetCurrentMethod().DeclaringType?.Name))
            {
                using (TodoDbContext todoDbContext =
                    testWebApplicationFactory.Server.Services.GetRequiredService<TodoDbContext>())
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
        }
    }
}