using bookbox.Entities;

namespace bookbox.Services.Interfaces
{
    public interface IUserService
    {
        Task<Users> CreateUserAsync(Users user);
        Task<int> GetActiveUsersCountAsync();
    }
}