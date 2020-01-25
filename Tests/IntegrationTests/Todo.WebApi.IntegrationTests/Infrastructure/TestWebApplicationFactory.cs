using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using Todo.Persistence;

namespace Todo.WebApi.Infrastructure
{
    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private const string EnvironmentName = "IntegrationTests";

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

                SetupTestDbContext(services);
                SetupTestDatabase(services);
            });
        }

        private void SetupTestDbContext(IServiceCollection services)
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

        private static void SetupTestDatabase(IServiceCollection services)
        {
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            using (IServiceScope serviceScope = serviceProvider.CreateScope())
            {
                TodoDbContext todoDbContext = serviceScope.ServiceProvider.GetRequiredService<TodoDbContext>();
                todoDbContext.Database.EnsureDeleted();
                todoDbContext.Database.Migrate();
            }
        }
    }
}