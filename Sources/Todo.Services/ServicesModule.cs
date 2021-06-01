namespace Todo.Services
{
    using Autofac;

    using Persistence;

    using Security;

    using TodoItemLifecycleManagement;

    /// <summary>
    /// Configures business related services used by this application.
    /// </summary>
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<PersistenceModule>();

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
