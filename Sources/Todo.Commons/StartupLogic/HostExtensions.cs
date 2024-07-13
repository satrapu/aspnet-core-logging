namespace Todo.Commons.StartupLogic
{
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Contains extension methods applicable to <seealso cref="IHost"/> objects.
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Runs application startup logic (e.g., database migrations), then the host.
        /// </summary>
        /// <param name="host">The <seealso cref="IHost"/> instance to run.</param>
        public static async Task RunWithTasksAsync(this IHost host)
        {
            await using AsyncServiceScope asyncServiceScope = host.Services.CreateAsyncScope();
            await asyncServiceScope.ServiceProvider.GetRequiredService<IStartupLogicTaskExecutor>().ExecuteAsync();
            await host.RunAsync();
        }
    }
}
