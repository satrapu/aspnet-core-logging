namespace Todo.ApplicationFlows.DependencyInjection
{
    using ApplicationFlows;
    using Security;
    using TodoItems;

    using Autofac;

    using Microsoft.Extensions.Configuration;

    using ApplicationEvents;
    using Todo.Commons.ApplicationEvents;
    using Todo.Services.DependencyInjection;

    /// <summary>
    /// Configures application flow related services used by this application.
    /// </summary>
    public class ApplicationFlowsModule : Module
    {
        private const string ApplicationFlowsConfigurationSectionName = "ApplicationFlows";

        /// <summary>
        /// Gets or sets the name of the environment where this application runs.
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets the configuration used by this application.
        /// </summary>
        public IConfiguration ApplicationConfiguration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new ServicesModule
            {
                EnvironmentName = EnvironmentName
            });

            builder
                .Register(componentContext =>
                ApplicationConfiguration.GetSection(ApplicationFlowsConfigurationSectionName).Get<ApplicationFlowOptions>())
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
                .RegisterType<ApplicationStartedEventNotifier>()
                .As<IApplicationStartedEventNotifier>()
                .SingleInstance();

            builder
                .RegisterType<RunDatabaseMigrations>()
                .As<IApplicationStartedEventListener>()
                .SingleInstance();
        }
    }
}
