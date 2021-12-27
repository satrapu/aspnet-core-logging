namespace Todo.ApplicationFlows.ApplicationEvents
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;

    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Todo.Commons.ApplicationEvents;

    /// <summary>
    /// An <see cref="IApplicationStartedEventNotifier"/> implementation which runs all registered
    /// <see cref="IApplicationStartedEventListener"/> instances.
    /// </summary>
    public class ApplicationStartedEventNotifier : IApplicationStartedEventNotifier
    {
        private const string FlowName = "Events/ApplicationStarted/NotifyListeners";

        private static readonly IPrincipal Principal =
            new GenericPrincipal(new GenericIdentity("application-started-event-notifier"), Array.Empty<string>());

        private readonly IEnumerable<IApplicationStartedEventListener> eventListeners;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ILogger logger;

        public ApplicationStartedEventNotifier(IEnumerable<IApplicationStartedEventListener> eventListeners,
            IHostApplicationLifetime hostApplicationLifetime, ILogger<ApplicationStartedEventNotifier> logger)
        {
            this.eventListeners = eventListeners ?? throw new ArgumentNullException(nameof(eventListeners));

            this.hostApplicationLifetime = hostApplicationLifetime ??
                                           throw new ArgumentNullException(nameof(hostApplicationLifetime));

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Notify()
        {
            SimpleApplicationFlow.Execute(FlowName, InternalNotify, Principal, logger);
        }

        private void InternalNotify()
        {
            try
            {
                foreach (IApplicationStartedEventListener eventListener in eventListeners)
                {
                    string eventListenerName = eventListener.GetType().AssemblyQualifiedName;

                    logger.LogInformation(
                        "Application started event listener: [{ApplicationStartedEventListener}] is about to run ...",
                        eventListenerName);

                    eventListener.OnApplicationStarted();

                    logger.LogInformation(
                        "Application started event listener: [{ApplicationStartedEventListener}] has run successfully",
                        eventListenerName);
                }
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception,
                    "An error has occurred while executing application started event listeners");

                hostApplicationLifetime.StopApplication();
            }
        }
    }
}
