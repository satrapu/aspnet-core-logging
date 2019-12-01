using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Todo.WebApi.Authorization;

namespace Todo.WebApi.Authentication
{
    /// <summary>
    /// Sets the current user to a hard-coded value.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class HardCodedUserMiddleware
    {
        private readonly RequestDelegate nextRequestDelegate;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new instance of the <see cref="HardCodedUserMiddleware"/> class.
        /// </summary>
        /// <param name="nextRequestDelegate"></param>
        /// <param name="logger"></param>
        public HardCodedUserMiddleware(RequestDelegate nextRequestDelegate, ILogger<HardCodedUserMiddleware> logger)
        {
            this.nextRequestDelegate = nextRequestDelegate ?? throw new ArgumentNullException(nameof(nextRequestDelegate));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes the given <paramref name="httpContext"/> object.
        /// </summary>
        /// <param name="httpContext">The current HTTP context to be processed.</param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(HttpContext httpContext)
        {
            logger.LogDebug("Setting hard-coded user for development purposes");
            var hardCodedClaimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                  new Claim(ClaimTypes.NameIdentifier, "190A935B-B718-4D1A-90E3-391435718918")
                , new Claim(ClaimTypes.Name, "satrapu")
                , new Claim(ClaimTypes.Email, "satrapu@noserver.ro")
                , new Claim(ClaimTypes.Role, ApplicationRoles.SuperUser.ToString())
            });
            httpContext.User = new ClaimsPrincipal(hardCodedClaimsIdentity);
            await nextRequestDelegate(httpContext).ConfigureAwait(false);
        }
    }
}
