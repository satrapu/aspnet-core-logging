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
        private string EnvironmentName { get; }

        public ServicesModule(string environmentName)
        {
            EnvironmentName = environmentName;
        }

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
