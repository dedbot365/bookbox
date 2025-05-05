using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using bookbox.Entities;
using bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace bookbox.Controllers
{
    [ApiController]
    [Route("api/register[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="user">User information</param>
        /// <returns>The newly created user</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Users))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateUser(Users user)
        {
            // Debug info
            Console.WriteLine($"Received registration for: {user.Email}");
            Console.WriteLine($"Addresses count: {user.Addresses?.Count ?? 0}");
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if user already exists
                var existsCheck = await _userService.CheckUserExistsAsync(user.Username, user.Email);
                if (existsCheck.userExists)
                {
                    string message;
                    if (existsCheck.usernameExists && existsCheck.emailExists)
                        message = "Registration failed. Both username and email already exist.";
                    else if (existsCheck.usernameExists)
                        message = "Registration failed. Username already exists.";
                    else
                        message = "Registration failed. Email already exists.";
                    
                    return Conflict(new { message });
                }

                // Do NOT create a new address - use what was sent from frontend
                if (user.Addresses == null)
                {
                    user.Addresses = new List<Address>();
                }
                
                // Don't set User property on addresses - it will cause circular reference
                foreach (var address in user.Addresses)
                {
                    // Ensure address has required properties
                    if (string.IsNullOrEmpty(address.Address1))
                    {
                        return BadRequest(new { message = "Address Line 1 is required" });
                    }
                    if (string.IsNullOrEmpty(address.City))
                    {
                        return BadRequest(new { message = "City is required" });
                    }
                    if (string.IsNullOrEmpty(address.State))
                    {
                        return BadRequest(new { message = "State is required" });
                    }
                    if (string.IsNullOrEmpty(address.Zip))
                    {
                        return BadRequest(new { message = "Zip code is required" });
                    }
                    
                    // Clear any incoming UserId to avoid conflicts
                    address.UserId = Guid.Empty; // Changed from int 0 to Guid.Empty
                    
                    // Important: Don't set address.User here
                }
                
                var createdUser = await _userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Failed to register user", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Users))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(Guid id)
        {
            // Updated to use Guid
            return Ok(new { Id = id, Message = "User found" });
        }

        [HttpGet("active-count")]
        public async Task<IActionResult> GetActiveUsersCount()
        {
            // This should be replaced with proper authorization check
            // For now, we're just checking the current user's admin status
            var currentUser = GetCurrentUser();
            if (currentUser == null || !currentUser.IsAdmin)
            {
                return Forbid();
            }

            var count = await _userService.GetActiveUsersCountAsync();
            return Ok(new { ActiveUsersCount = count });
        }

        // Helper method to get current user - replace with your authentication implementation
        private Users GetCurrentUser()
        {
            // In a real implementation, you would get the user from your auth context
            // This is just a placeholder
            return null;
        }
    }
}
