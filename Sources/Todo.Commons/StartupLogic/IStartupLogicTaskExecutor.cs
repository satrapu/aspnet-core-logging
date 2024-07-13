namespace Todo.Commons.StartupLogic
{
    using System.Threading.Tasks;

    /// <summary>
    /// Executes application startup logic by invoking all registered <seealso cref="IStartupLogicTask"/> instances.
    /// </summary>
    public interface IStartupLogicTaskExecutor
    {
        /// <summary>
        /// Executes all registered <seealso cref="IStartupLogicTask"/> instances.
        /// </summary>
        Task ExecuteAsync();
    }
}
