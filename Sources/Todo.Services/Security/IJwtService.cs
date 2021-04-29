namespace Todo.Services.Security
{
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public interface IJwtService
    {
        Task<JwtInfo> GenerateJwtAsync(GenerateJwtInfo generateJwtInfo);
    }
}
