using System;
using Bookbox.Data;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Bookbox.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> RegisterAsync(User user, string password, Address address)
        {
            if (await UserExistsAsync(user.Username, user.Email))
                return null;

            // Hash password
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            
            // Use a transaction to ensure both user and address are saved
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Add user first
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                // Associate address with user
                address.UserId = user.UserId;
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                
                return user;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<User?> LoginAsync(string usernameOrEmail, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null;

            return user;
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            return await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
        }

        // Add this new method to the UserService class
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
    }
}
