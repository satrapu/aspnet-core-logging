namespace Todo.Commons
{
    /// <summary>
    /// Listens to application started event.
    /// </summary>
    public interface IApplicationStartedEventListener
    {
        /// <summary>
        /// Execute logic during application started event.
        /// </summary>
        public void OnApplicationStarted();
    }
}
