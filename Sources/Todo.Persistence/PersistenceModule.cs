namespace Todo.Persistence
{
    using System;

    using Autofac;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Configures persistence related services used by this application.
    /// </summary>
    public class PersistenceModule : Module
    {
        private bool IsDevelopmentEnvironment { get; }

        private string ConnectionName { get; }

        public PersistenceModule(string connectionName, bool isDevelopmentEnvironment)
        {
            ConnectionName = connectionName;
            IsDevelopmentEnvironment = isDevelopmentEnvironment;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(componentContext =>
                {
                    IServiceProvider serviceProvider = componentContext.Resolve<IServiceProvider>();
                    ILoggerFactory loggerFactory = componentContext.Resolve<ILoggerFactory>();

                    IConfiguration configuration = componentContext.Resolve<IConfiguration>();
                    string connectionString = configuration.GetConnectionString(ConnectionName);

                    var dbContextOptions = new DbContextOptions<TodoDbContext>();

                    var dbContextOptionsBuilder = new DbContextOptionsBuilder<TodoDbContext>(dbContextOptions)
                        .UseApplicationServiceProvider(serviceProvider)
                        .UseNpgsql(connectionString)
                        .UseLoggerFactory(loggerFactory);

                    if (IsDevelopmentEnvironment)
                    {
                        dbContextOptionsBuilder.EnableSensitiveDataLogging();
                        dbContextOptionsBuilder.EnableDetailedErrors();
                    }

                    return dbContextOptionsBuilder.Options;
                })
                .As<DbContextOptions<TodoDbContext>>()
                .InstancePerLifetimeScope();

            builder
                .Register(context => context.Resolve<DbContextOptions<TodoDbContext>>())
                .As<DbContextOptions>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<TodoDbContext>()
                .AsSelf()
                .InstancePerLifetimeScope();
        }
    }
}
