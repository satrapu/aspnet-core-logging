namespace Todo.Services.DependencyInjection
{
    using Autofac;

    using Commons.Constants;

    using Security;

    using Todo.Persistence.DependencyInjection;

    using TodoItemManagement;

    /// <summary>
    /// Configures business related services used by this application.
    /// </summary>
    public class ServicesModule : Module
    {
        /// <summary>
        /// Gets or sets the name of the environment where this application runs.
        /// </summary>
        public string EnvironmentName { get; init; }

        protected override void Load(ContainerBuilder builder)
        {
            bool isDevelopmentEnvironment = EnvironmentNames.Development.Equals(EnvironmentName);
            bool isIntegrationTestsEnvironment = EnvironmentNames.IntegrationTests.Equals(EnvironmentName);
            bool isAcceptanceTestsEnvironment = EnvironmentNames.AcceptanceTests.Equals(EnvironmentName);

            var persistenceModule = new PersistenceModule
            {
                ConnectionStringName = GetConnectionStringNameByEnvironment(EnvironmentName),
                EnableDetailedErrors = isDevelopmentEnvironment || isIntegrationTestsEnvironment || isAcceptanceTestsEnvironment,
                EnableSensitiveDataLogging = isDevelopmentEnvironment || isIntegrationTestsEnvironment || isAcceptanceTestsEnvironment
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

        private static string GetConnectionStringNameByEnvironment(string environmentName)
        {
            return environmentName switch
            {
                EnvironmentNames.AcceptanceTests => ConnectionStrings.UsedByAcceptanceTests,
                EnvironmentNames.IntegrationTests => ConnectionStrings.UsedByIntegrationTests,
                _ => ConnectionStrings.UsedByApplication
            };
        }
    }
}
