using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Todo.Persistence;

namespace Todo.WebApi.Infrastructure
{
    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private const string EnvironmentName = "IntegrationTests";
        private const string BeforeFirstDatabaseMigration = "0";
        private static bool isDatabaseSetup;
        private static readonly object Padlock = new object();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var configurationBuilder = new ConfigurationBuilder();
            IConfigurationRoot testConfiguration = configurationBuilder.AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{EnvironmentName}.json", false)
                .AddEnvironmentVariables()
                .Build();

            builder.UseConfiguration(testConfiguration);
            builder.UseEnvironment(EnvironmentName);
            builder.ConfigureTestServices(services =>
            {
                services.AddMvc(options =>
                {
                    options.Filters.Add(new AllowAnonymousFilter());
                    options.Filters.Add(new InjectTestUserFilter());
                });

                SetupDbContext(services);
                SetupDatabase(services);
            });
        }

        private void SetupDbContext(IServiceCollection services)
        {
            ServiceDescriptor dbContextServiceDescriptor = services.SingleOrDefault(serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(DbContextOptions<TodoDbContext>));

            if (dbContextServiceDescriptor != null)
            {
                services.Remove(dbContextServiceDescriptor);
            }

            services.AddDbContext<TodoDbContext>((serviceProvider, dbContextOptionsBuilder) =>
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var testConnectionString = configuration.GetConnectionString("TodoForIntegrationTests");
                dbContextOptionsBuilder.UseNpgsql(testConnectionString)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
            });
        }

        private static void SetupDatabase(IServiceCollection services)
        {
            if (!isDatabaseSetup)
            {
                lock (Padlock)
                {
                    if (!isDatabaseSetup)
                    {
                        ServiceProvider serviceProvider = services.BuildServiceProvider();

                        using (IServiceScope serviceScope = serviceProvider.CreateScope())
                        {
                            TodoDbContext todoDbContext =
                                serviceScope.ServiceProvider.GetRequiredService<TodoDbContext>();
                            ILogger logger = serviceProvider.GetRequiredService<ILogger<TestWebApplicationFactory>>();

                            try
                            {
                                RunMigrations(todoDbContext, logger);
                            }
                            catch (Exception exception)
                            {
                                logger.LogError("Failed to migrate test database", exception);
                                throw;
                            }

                            isDatabaseSetup = true;
                        }
                    }
                }
            }
        }

        private static void RunMigrations(TodoDbContext todoDbContext, ILogger logger)
        {
            logger.LogInformation("About to delete test database ...");
            bool hasDeleteDatabase = todoDbContext.Database.EnsureDeleted();

            if (hasDeleteDatabase)
            {
                logger.LogInformation("Test database has been successfully deleted");
            }
            else
            {
                logger.LogInformation("Could not find any test database to delete");
            }

            IMigrator databaseMigrator = todoDbContext.GetInfrastructure().GetRequiredService<IMigrator>();

            logger.LogInformation("About to run test database migrations (1st time) ...");
            databaseMigrator.Migrate();
            logger.LogInformation("Test database migrations have been successfully run (1st time)");

            logger.LogInformation("About to revert test database migrations ...");
            // Revert migrations by using a special migration identifier.
            // See more here: https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet#dotnet-ef-database-update.
            databaseMigrator.Migrate(BeforeFirstDatabaseMigration);
            logger.LogInformation("Test database migrations have been successfully reverted");

            logger.LogInformation("About to run test database migrations (2nd time) ...");
            databaseMigrator.Migrate();
            logger.LogInformation("Test database migrations have been successfully run (2nd time)");
            logger.LogInformation("Test database has been successfully setup");
        }
    }
}