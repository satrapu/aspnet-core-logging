namespace Todo.ApplicationFlows
{
    using Autofac;

    using Microsoft.Extensions.Configuration;

    using Security;

    using Services;

    using TodoItems;

    /// <summary>
    /// Configures application flow related services used by this application.
    /// </summary>
    public class ApplicationFlowsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<ServicesModule>();

            // ReSharper disable once SettingNotFoundInConfiguration
            builder
                .Register(componentContext => componentContext
                    .Resolve<IConfiguration>()
                    .GetSection("ApplicationFlows")
                    .Get<ApplicationFlowOptions>())
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
