namespace Todo.Persistence
{
    using System;
    using System.Collections.Generic;

    using Commons;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Runs database migrations during application started event.
    /// </summary>
    public class RunDatabaseMigrations : IApplicationStartedEventListener
    {
        private readonly TodoDbContext todoDbContext;
        private readonly IConfiguration configuration;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ILogger logger;

        public RunDatabaseMigrations(
            TodoDbContext todoDbContext,
            IConfiguration configuration,
            IHostApplicationLifetime hostApplicationLifetime,
            ILogger<RunDatabaseMigrations> logger)
        {
            this.todoDbContext = todoDbContext ?? throw new ArgumentNullException(nameof(todoDbContext));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.hostApplicationLifetime = hostApplicationLifetime ??
                                           throw new ArgumentNullException(nameof(hostApplicationLifetime));

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnApplicationStarted()
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                [Constants.ConversationId] = Guid.NewGuid().ToString("N"),
                [Constants.ApplicationFlowName] = "Database/RunMigrations"
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
                logger.LogCritical(exception,
                    "Failed to migrate database {DatabaseName}; application will stop immediately", database);

                hostApplicationLifetime.StopApplication();
            }
        }
    }
}
