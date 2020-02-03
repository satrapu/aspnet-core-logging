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
using Npgsql;
using Todo.Persistence;

namespace Todo.WebApi.Infrastructure
{
    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private const string EnvironmentName = "IntegrationTests";
        private readonly string testDatabaseName;

        public TestWebApplicationFactory(string applicationName)
        {
            testDatabaseName = $"it--{applicationName}";
        }

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
                    options.Filters.Add(new WebApi.Infrastructure.InjectTestUserFilter());
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
                var connectionStringBuilder = new NpgsqlConnectionStringBuilder(testConnectionString)
                {
                    Database = testDatabaseName
                };
                dbContextOptionsBuilder.UseNpgsql(connectionStringBuilder.ConnectionString)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
            });
        }

        private static void SetupDatabase(IServiceCollection services)
        {
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            using (IServiceScope serviceScope = serviceProvider.CreateScope())
            {
                TodoDbContext todoDbContext =
                    serviceScope.ServiceProvider.GetRequiredService<TodoDbContext>();
                ILogger logger = serviceProvider.GetRequiredService<ILogger<TestWebApplicationFactory>>();
                string databaseName = todoDbContext.Database.GetDbConnection().Database;
                logger.LogInformation("About to setup test database {TestDatabaseName} ...",
                    databaseName);

                try
                {
                    RunMigrations(databaseName, todoDbContext, logger);
                }
                catch (Exception exception)
                {
                    logger.LogError("Failed to run migrations against test database {TestDatabaseName}", exception,
                        databaseName);
                    throw;
                }

                logger.LogInformation("Test database {TestDatabaseName} has been successfully setup", databaseName);
            }
        }

        private static void RunMigrations(string databaseName, TodoDbContext todoDbContext, ILogger logger)
        {
            logger.LogInformation("About to delete test database {TestDatabaseName} ...", databaseName);
            bool hasDeleteDatabase = todoDbContext.Database.EnsureDeleted();

            if (hasDeleteDatabase)
            {
                logger.LogInformation("Test database {TestDatabaseName} has been successfully deleted", databaseName);
            }
            else
            {
                logger.LogInformation("Could not find any test database {TestDatabaseName} to delete", databaseName);
            }

            IMigrator databaseMigrator = todoDbContext.GetInfrastructure().GetRequiredService<IMigrator>();

            logger.LogInformation("About to run migrations against test database {TestDatabaseName} ...", databaseName);
            databaseMigrator.Migrate();
            logger.LogInformation("Migrations have been successfully run against test database {TestDatabaseName}",
                databaseName);
        }
    }
}