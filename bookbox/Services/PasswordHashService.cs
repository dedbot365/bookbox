using System;
using BCrypt.Net;
using bookbox.Services.Interfaces;

namespace bookbox.Services
{
    public class PasswordHashService : IPasswordHashService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
            }
            catch
            {
                // If the hash format is invalid, verification fails
                return false;
            }
        }
    }
}
