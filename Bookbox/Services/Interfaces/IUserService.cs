using Bookbox.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookbox.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> RegisterAsync(User user, string password, Address address);
        Task<User?> LoginAsync(string usernameOrEmail, string password);
        Task<bool> UserExistsAsync(string username, string email);
        
        // Add this new method to match the implementation in UserService
        Task<List<User>> GetAllUsersAsync();
    }
}