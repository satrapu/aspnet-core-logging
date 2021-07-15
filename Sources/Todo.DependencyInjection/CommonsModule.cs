namespace Todo.DependencyInjection
{
    using Autofac;

    using Commons.ApplicationEvents;

    /// <summary>
    /// Configures services offering common functionality to be used by this application.
    /// </summary>
    public class CommonsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ApplicationStartedEventNotifier>()
                .As<IApplicationStartedEventNotifier>()
                .SingleInstance();
        }
    }
}
