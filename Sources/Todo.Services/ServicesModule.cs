namespace Todo.Services
{
    using Autofac;

    using Security;

    using TodoItemLifecycleManagement;

    /// <summary>
    /// Configures business related services used by this application.
    /// </summary>
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // satrapu 2021-06-13: Should register PersistenceModule here, but doing this
            // while the module is declared inside autofac.json file means the RunDatabaseMigrations class will be
            // executed twice, since the module will be registered twice (once here and once inside the JSON file).
            // Need to find a better way of registering a module which requires complex setup!
            //// builder.RegisterModule<PersistenceModule>();

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
