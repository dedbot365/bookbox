using System;
using System.Threading.Tasks;
using bookbox.Entities;
using bookbox.DTOs;

namespace bookbox.Services.Interfaces
{
    public interface IUserService
    {
        Task<Users> CreateUserAsync(Users user);
        Task<int> GetActiveUsersCountAsync();
        Task<(bool userExists, bool usernameExists, bool emailExists)> CheckUserExistsAsync(string username, string email);
        Task<Users> AuthenticateAsync(string email, string password);
    }
}