using System;
using System.Threading.Tasks;
using bookbox.Models;
using bookbox.DTOs;

namespace bookbox.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user);
        Task<int> GetActiveUsersCountAsync();
        Task<(bool userExists, bool usernameExists, bool emailExists)> CheckUserExistsAsync(string username, string email);
        Task<User> AuthenticateAsync(string email, string password);
    }
}