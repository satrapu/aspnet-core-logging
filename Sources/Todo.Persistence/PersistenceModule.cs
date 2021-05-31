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
        private bool IsRunningLocally { get; }

        private string ConnectionStringName { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="PersistenceModule"/> class.
        /// </summary>
        /// <param name="connectionStringName">The name of the connection string to use when communicating with
        /// the underlying RDBMS.</param>
        /// <param name="isRunningLocally">Value representing whether the services configured via this module will run
        /// inside a development environment (e.g. locally or inside a CI pipeline).</param>
        public PersistenceModule(string connectionStringName, bool isRunningLocally)
        {
            ConnectionStringName = connectionStringName;
            IsRunningLocally = isRunningLocally;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(componentContext =>
                {
                    IServiceProvider serviceProvider = componentContext.Resolve<IServiceProvider>();
                    ILoggerFactory loggerFactory = componentContext.Resolve<ILoggerFactory>();

                    IConfiguration configuration = componentContext.Resolve<IConfiguration>();
                    string connectionString = configuration.GetConnectionString(ConnectionStringName);

                    var dbContextOptions = new DbContextOptions<TodoDbContext>();

                    var dbContextOptionsBuilder = new DbContextOptionsBuilder<TodoDbContext>(dbContextOptions)
                        .UseApplicationServiceProvider(serviceProvider)
                        .UseNpgsql(connectionString)
                        .UseLoggerFactory(loggerFactory);

                    if (IsRunningLocally)
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
