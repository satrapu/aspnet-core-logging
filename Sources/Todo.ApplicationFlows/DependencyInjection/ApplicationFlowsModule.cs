namespace Todo.ApplicationFlows.DependencyInjection
{
    using ApplicationEvents;

    using ApplicationFlows;

    using Autofac;

    using Commons.StartupLogic;

    using Microsoft.Extensions.Configuration;

    using Security;

    using Todo.Services.DependencyInjection;

    using TodoItems;

    /// <summary>
    /// Configures application flow related services used by this application.
    /// </summary>
    public class ApplicationFlowsModule : Module
    {
        private const string ApplicationFlowsConfigurationSectionName = "ApplicationFlows";

        /// <summary>
        /// Gets or sets the name of the environment where this application runs.
        /// </summary>
        public string EnvironmentName { get; init; }

        /// <summary>
        /// Gets or sets the configuration used by this application.
        /// </summary>
        public IConfiguration ApplicationConfiguration { get; init; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new ServicesModule
            {
                EnvironmentName = EnvironmentName
            });

            builder
                .Register(_ => ApplicationConfiguration.GetSection(ApplicationFlowsConfigurationSectionName).Get<ApplicationFlowOptions>())
                .SingleInstance();

            builder
                .RegisterType<GenerateJwtFlow>()
                .As<IGenerateJwtFlow>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<FetchTodoItemsFlow>()
                .As<IFetchTodoItemsFlow>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<FetchTodoItemByIdFlow>()
                .As<IFetchTodoItemByIdFlow>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<AddTodoItemFlow>()
                .As<IAddTodoItemFlow>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<UpdateTodoItemFlow>()
                .As<IUpdateTodoItemFlow>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<DeleteTodoItemFlow>()
                .As<IDeleteTodoItemFlow>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<StartupLogicTaskExecutor>()
                .As<IStartupLogicTaskExecutor>()
                .SingleInstance();

            builder
                .RegisterType<RunDatabaseMigrations>()
                .As<IStartupLogicTask>()
                .SingleInstance();
        }
    }
}
