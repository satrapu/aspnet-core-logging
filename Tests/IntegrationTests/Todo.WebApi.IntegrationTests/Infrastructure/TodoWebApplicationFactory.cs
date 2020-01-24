using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Todo.Persistence;
using Todo.Persistence.Entities;
using Todo.Services;

namespace Todo.WebApi.Infrastructure
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TodoWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            var testServer = base.CreateServer(builder);
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
    }
}