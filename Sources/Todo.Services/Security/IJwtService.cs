using System.Threading.Tasks;

namespace Todo.Services.Security
{
    /// <summary>
    /// 
    /// </summary>
    public interface IJwtService
    {
        Task<JwtInfo> GenerateJwtAsync(GenerateJwtInfo generateJwtInfo);
    }
}