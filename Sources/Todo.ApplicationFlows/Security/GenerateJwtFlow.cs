namespace Todo.ApplicationFlows.Security
{
    using System;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Todo.Services.Security;

    /// <summary>
    /// An <see cref="IGenerateJwtFlow"/> implementation.
    /// </summary>
    public class GenerateJwtFlow : NonTransactionalBaseApplicationFlow<GenerateJwtInfo, JwtInfo>, IGenerateJwtFlow
    {
        private readonly IJwtService jwtService;

        public GenerateJwtFlow(IJwtService jwtService, ILogger<GenerateJwtFlow> logger) :
            base("Security/GenerateJwt", logger)
        {
            this.jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        protected override async Task<JwtInfo> ExecuteFlowStepsAsync(GenerateJwtInfo input, IPrincipal flowInitiator)
        {
            return await jwtService.GenerateJwtAsync(input);
        }
    }
}
