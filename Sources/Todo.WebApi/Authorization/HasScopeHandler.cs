using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Todo.WebApi.Authorization
{
    /// <summary>
    /// Based on: https://auth0.com/docs/quickstart/backend/aspnet-core-webapi#validate-scopes.
    /// </summary>
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(claim => claim.Type == "scope" && claim.Issuer == requirement.Issuer))
            {
                return Task.CompletedTask;
            }

            // Split the scopes string into an array
            var scopes = context.User.FindFirst(claim => claim.Type == "scope" && claim.Issuer == requirement.Issuer)
                .Value.Split(' ');

            // Succeed if the scope array contains the required scope
            if (scopes.Any(scope => scope == requirement.Scope))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}