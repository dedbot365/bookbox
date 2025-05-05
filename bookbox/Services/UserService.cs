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
            // Save the addresses temporarily
            var addresses = user.Addresses?.ToList() ?? new List<Address>();
            
            // Clear the addresses collection to avoid circular reference
            user.Addresses = new List<Address>();
            
            // Add user without addresses first
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Now add addresses with the correct UserId
            if (addresses.Any())
            {
                foreach (var address in addresses)
                {
                    // Explicitly set navigation property first
                    address.User = user;
                    address.UserId = user.Id;
                    
                    // Make sure we're tracking the entity correctly
                    var entry = _context.Entry(address);
                    if (entry.State == EntityState.Detached)
                    {
                        _context.Addresses.Add(address);
                    }
                }
                await _context.SaveChangesAsync();
            }
            
            // Return the complete user with addresses
            var createdUser = await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == user.Id);
                
            if (createdUser == null)
            {
                throw new Exception($"User with ID {user.Id} was not found after creation.");
            }
            
            return createdUser;
        }
        
        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.IsActive);
        }

        public async Task<(bool userExists, bool usernameExists, bool emailExists)> CheckUserExistsAsync(string username, string email)
        {
            bool usernameExists = await _context.Users.AnyAsync(u => u.Username == username);
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == email);
            bool userExists = usernameExists || emailExists;
            
            return (userExists, usernameExists, emailExists);
        }
    }
}
