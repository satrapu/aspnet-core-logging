namespace Todo.Commons.ApplicationEvents
{
    using System.Threading.Tasks;

    /// <summary>
    /// Listens to application started event.
    /// </summary>
    public interface IApplicationStartedEventListener
    {
        /// <summary>
        /// Execute logic during application started event.
        /// </summary>
        public Task OnApplicationStartedAsync();
    }
}
