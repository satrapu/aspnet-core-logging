namespace Todo.Services.DependencyInjection
{
    using Autofac;

    using Commons.Constants;

    using Services.Security;
    using Services.TodoItemManagement;

    using Todo.Persistence.DependencyInjection;

    /// <summary>
    /// Configures business related services used by this application.
    /// </summary>
    public class ServicesModule : Module
    {
        /// <summary>
        /// Gets or sets the name of the environment where this application runs.
        /// </summary>
        public string EnvironmentName { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            bool isDevelopmentEnvironment = EnvironmentNames.Development.Equals(EnvironmentName);
            bool isIntegrationTestsEnvironment = EnvironmentNames.IntegrationTests.Equals(EnvironmentName);

            var persistenceModule = new PersistenceModule
            {
                ConnectionStringName = isIntegrationTestsEnvironment
                    ? ConnectionStrings.UsedByIntegrationTests
                    : ConnectionStrings.UsedByApplication,
                EnableDetailedErrors = isDevelopmentEnvironment || isIntegrationTestsEnvironment,
                EnableSensitiveDataLogging = isDevelopmentEnvironment || isIntegrationTestsEnvironment
            };

            builder.RegisterModule(persistenceModule);

            builder
                .RegisterType<JwtService>()
                .As<IJwtService>()
                .SingleInstance();

            builder
                .RegisterType<TodoItemService>()
                .As<ITodoItemService>()
                .InstancePerLifetimeScope();
        }
    }
}
