using System.Collections.Generic;
using Todo.Persistence;

namespace Todo.Services
{
    public interface IDatabaseSeeder
    {
        /// <summary>
        /// Ensures the database exposed via the given <paramref name="todoDbContext"/> exists and it contains some initial seedingData.
        /// </summary>
        /// <param name="todoDbContext">The <see cref="TodoDbContext"/> instance used for accessing the underlying database.</param>
        /// <param name="seedingData">The data used for seeding the underlying database.</param>
        /// <returns>True, if seeding took place; false, otherwise.</returns>
        bool Seed(TodoDbContext todoDbContext, IEnumerable<TodoItem> seedingData);
    }
}
