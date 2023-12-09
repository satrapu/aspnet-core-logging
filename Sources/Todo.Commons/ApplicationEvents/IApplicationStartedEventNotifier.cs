namespace Todo.Commons.ApplicationEvents
{
    using System.Threading.Tasks;

    /// <summary>
    /// Notifies any registered <see cref="IApplicationStartedEventListener"/> when application started event has
    /// occurred.
    /// </summary>
    public interface IApplicationStartedEventNotifier
    {
        /// <summary>
        /// Notifies listeners.
        /// </summary>
        public Task NotifyAsync();
    }
}
