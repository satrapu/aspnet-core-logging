namespace Todo.Persistence.DependencyInjection
{
    using System;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public static class ServiceCollectionExtensions
    {
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
                        customTestQuery: async (todoDbContext, token) =>
                        {
                            try
                            {
                                await
                                    todoDbContext.TodoItems
                                        .Select(x => x.Id)
                                        .FirstOrDefaultAsync(cancellationToken: token);

                                return true;
                            }
                            catch
                            {
                                return false;
                            }
                        })
                    .Services;
        }
    }
}
