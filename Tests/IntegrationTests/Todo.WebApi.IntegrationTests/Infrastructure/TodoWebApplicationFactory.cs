using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Todo.Persistence;

namespace Todo.WebApi.Infrastructure
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TodoWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            var testServer = base.CreateServer(builder);
            MigrateDatabase(testServer.Host.Services);
            return testServer;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddMvc(options =>
                {
                    options.Filters.Add(new AllowAnonymousFilter());
                    options.Filters.Add(new InjectTestUserFilter());
                });
            });
        }

        private static void MigrateDatabase(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var todoDbContext = serviceScope.ServiceProvider.GetRequiredService<TodoDbContext>();
                todoDbContext.Database.Migrate();
            }
        }
    }
}