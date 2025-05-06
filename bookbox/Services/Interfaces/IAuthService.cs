using Bookbox.Models;
using Bookbox.DTOs;

namespace Bookbox.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterUserAsync(RegisterDTO registerDto);
        Task<(bool Success, User? User)> AuthenticateUserAsync(LoginDTO loginDTO);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<bool> IsUsernameUniqueAsync(string username);
        string GetRoleFromUserType(int userType);
    }
}