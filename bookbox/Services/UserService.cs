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
            // Convert explicitly to UTC time for PostgreSQL compatibility
            if (user.DateOfBirth != default && user.DateOfBirth.Kind != DateTimeKind.Utc)
                user.DateOfBirth = DateTime.SpecifyKind(user.DateOfBirth, DateTimeKind.Utc);
            
            if (user.RegistrationDate != default && user.RegistrationDate.Kind != DateTimeKind.Utc)
                user.RegistrationDate = DateTime.SpecifyKind(user.RegistrationDate, DateTimeKind.Utc);
            
            // Save the addresses temporarily
            var addresses = user.Addresses?.ToList() ?? new List<Address>();
            
            // Clear the addresses collection to avoid circular reference
            user.Addresses = new List<Address>();
            
            // Add user without addresses first
            _context.Users.Add(user);
            
            try {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) {
                Console.WriteLine($"Error saving user: {ex.Message}");
                if (ex.InnerException != null) {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
            
            // Now add addresses with the correct UserId
            if (addresses.Any())
            {
                foreach (var address in addresses)
                {
                    // Reset the Id to 0 to let the database generate a new ID
                    address.Id = 0;
                    address.UserId = user.Id;
                    
                    _context.Addresses.Add(address);
                }
                await _context.SaveChangesAsync();
            }
            
            // Return the complete user with addresses
            var createdUser = await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == user.Id);
                
            return createdUser ?? throw new Exception($"User with ID {user.Id} was not found after creation.");
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
