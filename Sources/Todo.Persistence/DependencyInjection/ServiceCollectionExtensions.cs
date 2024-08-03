namespace Todo.Persistence.DependencyInjection
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public static class ServiceCollectionExtensions
    {
        private static readonly TimeSpan MaxWaitTimeForDbContextHealthCheck = TimeSpan.FromSeconds(2);

        public static IServiceCollection AddPersistenceHealthChecks(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            return
                services
                    .AddHealthChecks()
                    .AddDbContextCheck<TodoDbContext>
                    (
                        name: "Persistent storage",
                        failureStatus: HealthStatus.Unhealthy,
                        customTestQuery: (todoDbContext, _) => IsTodoDbContextHealthyAsync(todoDbContext)
                    )
                    .Services;
        }

        private static async Task<bool> IsTodoDbContextHealthyAsync(TodoDbContext todoDbContext)
        {
            using CancellationTokenSource cancellationTokenSource = new(delay: MaxWaitTimeForDbContextHealthCheck);

            try
            {
                await
                    todoDbContext.TodoItems
                        .Select(x => x.Id)
                        .FirstOrDefaultAsync(cancellationToken: cancellationTokenSource.Token)
                        .WaitAsync(timeout: MaxWaitTimeForDbContextHealthCheck);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
