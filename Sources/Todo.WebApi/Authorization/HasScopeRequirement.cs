using System;
using Microsoft.AspNetCore.Authorization;

namespace Todo.WebApi.Authorization
{
    /// <summary>
    /// Based on: https://auth0.com/docs/quickstart/backend/aspnet-core-webapi#validate-scopes.
    /// </summary>
    public class HasScopeRequirement : IAuthorizationRequirement
    {
        public string Issuer { get; }

        public string Scope { get; }

        public HasScopeRequirement(string scope, string issuer)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        }
    }
}