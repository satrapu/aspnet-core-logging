using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Todo.Persistence;

namespace Todo.Services
{
    public class DatabaseSeeder: IDatabaseSeeder
    {
        private readonly ILogger logger;

        public DatabaseSeeder(ILogger<DatabaseSeeder> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool Seed(TodoDbContext todoDbContext, IEnumerable<TodoItem> seedingData)
        {
            if (todoDbContext == null)
            {
                throw new ArgumentNullException(nameof(todoDbContext));
            }

            if (seedingData == null)
            {
                throw new ArgumentNullException(nameof(seedingData));
            }

            try
            {
                var hasDatabaseBeenCreated = todoDbContext.Database.EnsureCreated();

                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (hasDatabaseBeenCreated)
                {
                    logger.LogInformation("Database has been created");
                }
                else
                {
                    logger.LogInformation("Database has *not* been created");
                }

                if (todoDbContext.TodoItems.Any())
                {
                    logger.LogInformation("Database has *not* been seeded");
                    return false;
                }

                todoDbContext.TodoItems.AddRange(seedingData);
                todoDbContext.SaveChanges();

                logger.LogInformation("Database has been seeded");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "A database seeding error has occurred");
                throw;
            }
        }
    }
}