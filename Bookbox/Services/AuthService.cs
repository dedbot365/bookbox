using Bookbox.Data;
using Bookbox.DTOs;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Bookbox.Constants;

namespace Bookbox.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterUserAsync(RegisterDTO registerDto)
        {
            if (!await IsEmailUniqueAsync(registerDto.Email) || 
                !await IsUsernameUniqueAsync(registerDto.Username))
            {
                return false;
            }

            // Create user from DTO
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                ContactNo = registerDto.ContactNo,
                Gender = registerDto.Gender,
                UserType = Constants.UserType.Member, // Default as member
                RegisteredDate = DateTimeOffset.UtcNow,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
            };
            
            // Create address from DTO
            var address = new Address
            {
                Address1 = registerDto.Address1,
                Address2 = registerDto.Address2,
                City = registerDto.City,
                State = registerDto.State,
                Country = registerDto.Country
                // UserId will be set after user is created
            };

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
                
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            return !await _context.Users.AnyAsync(u => u.Username == username);
        }

        // Fix this method to match interface signature:
        public string GetRoleFromUserType(int userType)
        {
            return userType == (int)Constants.UserType.Admin ? "Admin" : "Member";
        }
        
        // Update the AuthenticateUserAsync method:
        public async Task<(bool Success, User? User)> AuthenticateUserAsync(LoginDTO loginDTO)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDTO.UsernameOrEmail || u.Username == loginDTO.UsernameOrEmail);

            if (user == null)
                return (false, null);

            bool verified = BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.Password);
            return (verified, verified ? user : null);
        }
    }
}