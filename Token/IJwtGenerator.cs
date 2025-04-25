
using Inventio.Models;

namespace inventio.Token
{
    public interface IJwtGenerator
    {
        Task<string> CreateToken(User user);
    }
}