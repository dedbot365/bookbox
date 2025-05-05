using System.Security.Claims;
using bookbox.Models;

namespace bookbox.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user, bool rememberMe = false);
        ClaimsPrincipal ValidateToken(string token);
        bool IsTokenValid(string token);
    }
}
