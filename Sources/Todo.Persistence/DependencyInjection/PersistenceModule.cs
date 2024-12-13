namespace Todo.Persistence.DependencyInjection
{
    using System;

    using Autofac;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using Persistence;

    /// <summary>
    /// Configures persistence related services used by this application.
    /// </summary>
    public class PersistenceModule : Module
    {
        /// <summary>
        /// Gets or sets whether Entity Framework Core will enable sensitive data logging.
        /// </summary>
        public bool EnableSensitiveDataLogging { get; set; }

        /// <summary>
        /// Gets or sets whether Entity Framework Core will enable detailed errors.
        /// </summary>
        public bool EnableDetailedErrors { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection string to be used when connecting to the underlying RDBMS.
        /// </summary>
        public string ConnectionStringName { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(componentContext =>
                {
                    IServiceProvider serviceProvider = componentContext.Resolve<IServiceProvider>();
                    ILoggerFactory loggerFactory = componentContext.Resolve<ILoggerFactory>();

                    IConfiguration configuration = componentContext.Resolve<IConfiguration>();
                    string connectionString = configuration.GetConnectionString(ConnectionStringName);

                    DbContextOptions<TodoDbContext> dbContextOptions = new();

                    DbContextOptionsBuilder<TodoDbContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<TodoDbContext>(dbContextOptions)
                        .UseApplicationServiceProvider(serviceProvider)
                        .UseNpgsql(connectionString)
                        .UseLoggerFactory(loggerFactory);

                    if (EnableDetailedErrors)
                    {
                        dbContextOptionsBuilder.EnableDetailedErrors();
                    }

                    if (EnableSensitiveDataLogging)
                    {
                        dbContextOptionsBuilder.EnableSensitiveDataLogging();
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
