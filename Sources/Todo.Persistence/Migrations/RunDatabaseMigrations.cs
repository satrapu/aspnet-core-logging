namespace Todo.Persistence.Migrations
{
    using System;
    using System.Collections.Generic;

    using Commons.ApplicationEvents;
    using Commons.Constants;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Runs database migrations during application started event.
    /// </summary>
    public class RunDatabaseMigrations : IApplicationStartedEventListener
    {
        private readonly TodoDbContext todoDbContext;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        public RunDatabaseMigrations(
            TodoDbContext todoDbContext,
            IConfiguration configuration,
            ILogger<RunDatabaseMigrations> logger)
        {
            this.todoDbContext = todoDbContext ?? throw new ArgumentNullException(nameof(todoDbContext));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnApplicationStarted()
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                [Logging.ConversationId] = Guid.NewGuid().ToString("N"),
                [Logging.ApplicationFlowName] = "Database/RunMigrations"
            }))
            {
                InternalRunDatabaseMigrations();
            }
        }

        private void InternalRunDatabaseMigrations()
        {
            string database = "<unknown>";

            try
            {
                // ReSharper disable once SettingNotFoundInConfiguration
                bool shouldMigrateDatabase = configuration.GetValue<bool>("MigrateDatabase");

                if (!shouldMigrateDatabase)
                {
                    logger.LogInformation("Migrating database has been turned off");

                    return;
                }

                logger.LogInformation("Migrating database has been turned on");

                database = todoDbContext.Database.GetDbConnection().Database;

                logger.LogInformation("About to migrate database {DatabaseName} ...", database);
                todoDbContext.Database.Migrate();
                logger.LogInformation("Database {DatabaseName} has been migrated successfully ", database);
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "Failed to migrate database {DatabaseName}", database);

                throw;
            }
        }
    }
}
