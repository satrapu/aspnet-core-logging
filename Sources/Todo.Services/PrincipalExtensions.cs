using System;
using System.Security.Principal;

namespace Todo.Services
{
    /// <summary>
    /// Contains extension methods applicable to <seealso cref="IPrincipal"/> instances.
    /// </summary>
    public static class PrincipalExtensions
    {
        /// <summary>
        /// Gets the name associated with the given <param name="principal"/> instance.
        /// </summary>
        /// <param name="principal">The <seealso cref="IPrincipal"/> instance whose name is to be fetched.</param>
        /// <returns>The name associated with the given <paramref name="principal"/>.</returns>
        public static string GetName(this IPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            if (principal.Identity == null)
            {
                throw new ArgumentNullException($"{nameof(principal)}.{nameof(principal.Identity)}");
            }

            return principal.Identity?.Name;
        }
    }
}