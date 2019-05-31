using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Todo.WebApi.Infrastructure
{
    public class InjectTestUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserSettings.UserId),
                new Claim(ClaimTypes.Name, TestUserSettings.Name),
                new Claim(ClaimTypes.Email, TestUserSettings.Email),
                new Claim(ClaimTypes.Role, "Admin")
            }));

            await next().ConfigureAwait(false);
        }
    }
}
