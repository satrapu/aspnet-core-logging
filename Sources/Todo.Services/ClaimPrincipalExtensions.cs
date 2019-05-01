using System;
using System.Security.Claims;

namespace Todo.Services
{
    public static class ClaimPrincipalExtensions
    {
        public  static string GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            var result = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value;
            return result;
        }
    }
}
