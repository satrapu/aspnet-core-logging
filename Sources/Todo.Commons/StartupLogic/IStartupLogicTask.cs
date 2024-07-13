namespace Todo.Commons.StartupLogic
{
    using System.Threading.Tasks;

    /// <summary>
    /// Executes logic during application startup.
    /// </summary>
    public interface IStartupLogicTask
    {
        /// <summary>
        /// Executes logic during application startup.
        /// </summary>
        Task ExecuteAsync();
    }
}
