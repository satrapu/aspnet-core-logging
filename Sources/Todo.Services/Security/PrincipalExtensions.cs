namespace Todo.Services.Security
{
    using System;
    using System.Security.Principal;

    /// <summary>
    /// Contains extension methods applicable to <see cref="IPrincipal"/> instances.
    /// </summary>
    public static class PrincipalExtensions
    {
        /// <summary>
        /// Gets the name associated with the given <param name="principal"/> instance.
        /// </summary>
        /// <param name="principal">The <see cref="IPrincipal"/> instance whose name is to be fetched.</param>
        /// <returns>The name associated with the given <paramref name="principal"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown in case the given <paramref name="principal"/> is null
        /// or its <see cref="IPrincipal.Identity"/> property is null.</exception>
        public static string GetName(this IPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            if (principal.Identity == null)
            {
                throw new ArgumentException("Principal identity cannot be null", nameof(principal));
            }

            return principal.Identity.Name;
        }

        /// <summary>
        /// Gets the name associated with the given <param name="principal"/> instance or a default value,
        /// in case <paramref name="principal"/> is null.
        /// </summary>
        /// <param name="principal">The <see cref="IPrincipal"/> instance whose name is to be fetched.</param>
        /// <param name="defaultName">The default value to use in case <paramref name="principal"/> is null.</param>
        /// <returns>The name associated with the given <paramref name="principal"/>.</returns>
        public static string GetNameOrDefault(this IPrincipal principal, string defaultName = "<unknown-principal>")
        {
            if (principal == null)
            {
                return defaultName;
            }

            return GetName(principal);
        }
    }
}
