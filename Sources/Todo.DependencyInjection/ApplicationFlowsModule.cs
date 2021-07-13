namespace Todo.DependencyInjection
{
    using ApplicationFlows;
    using ApplicationFlows.Security;
    using ApplicationFlows.TodoItems;

    using Autofac;

    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configures application flow related services used by this application.
    /// </summary>
    public class ApplicationFlowsModule : Module
    {
        public string EnvironmentName { get; set; }

        public IConfiguration ApplicationConfiguration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new ServicesModule
            {
                EnvironmentName = EnvironmentName
            });

            // ReSharper disable once SettingNotFoundInConfiguration
            builder
                .Register(componentContext =>
                    ApplicationConfiguration.GetSection("ApplicationFlows").Get<ApplicationFlowOptions>())
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
        }
    }
}
