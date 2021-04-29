namespace Todo.ApplicationFlows.Security
{
    using Todo.Services.Security;

    /// <summary>
    /// Application flow used for generating JSON web tokens needed for authentication and authorization purposes.
    /// </summary>
    public interface IGenerateJwtFlow : IApplicationFlow<GenerateJwtInfo, JwtInfo>
    {
    }
}
