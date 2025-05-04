using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using bookbox.Data;
using bookbox.Entities;
using bookbox.Services.Interfaces;

namespace bookbox.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Users> CreateUserAsync(Users user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        
        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.IsActive);
        }
    }
}
