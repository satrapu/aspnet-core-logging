namespace Todo.DependencyInjection
{
    using Autofac;

    using Commons;

    using Services.Security;
    using Services.TodoItemLifecycleManagement;

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
            bool isDevelopmentEnvironment = Constants.EnvironmentNames.Development.Equals(EnvironmentName);
            bool isIntegrationTestsEnvironment = Constants.EnvironmentNames.IntegrationTests.Equals(EnvironmentName);

            var persistenceModule = new PersistenceModule
            {
                ConnectionStringName = isIntegrationTestsEnvironment
                    ? "TodoForIntegrationTests"
                    : "Todo",
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
