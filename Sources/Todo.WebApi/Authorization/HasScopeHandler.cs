namespace Todo.WebApi.Authorization
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Based on: https://auth0.com/docs/quickstart/backend/aspnet-core-webapi#validate-scopes.
    /// </summary>
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            Claim scopeClaim = context.User.FindFirst(claim => claim.Type == "scope" && claim.Issuer == requirement.Issuer);

            if (scopeClaim != null)
            {
                string[] scopes = scopeClaim.Value.Split(separator: ' ');

                if (Array.Exists(scopes, scope => scope == requirement.Scope))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            context.Fail();
            return Task.CompletedTask;
        }
    }
}
