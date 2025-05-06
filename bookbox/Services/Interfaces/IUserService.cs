using Bookbox.Models;

namespace Bookbox.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> RegisterAsync(User user, string password, Address address);
        Task<User?> LoginAsync(string usernameOrEmail, string password);
        Task<bool> UserExistsAsync(string username, string email);
    }
}