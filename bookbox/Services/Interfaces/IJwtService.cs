using System.Security.Claims;
using bookbox.Entities;

namespace bookbox.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Users user, bool rememberMe = false);
        ClaimsPrincipal ValidateToken(string token);
        bool IsTokenValid(string token);
    }
}
