using Todo.Services.Security;

namespace Todo.ApplicationFlows.Security
{
    /// <summary>
    /// Application flow used for generating JSON web tokens needed for authentication and authorization purposes.
    /// </summary>
    public interface IGenerateJwtFlow : IApplicationFlow<GenerateJwtInfo, JwtInfo>
    {
    }
}