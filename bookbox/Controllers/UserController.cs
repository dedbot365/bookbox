using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using bookbox.Entities;
using bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using bookbox.DTOs;  // Add this line to import UserLoginDto
using System.Security.Claims;

namespace bookbox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public UserController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
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

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="loginDto">Login credentials with remember me option</param>
        /// <returns>User information with authentication token</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                var user = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password);
                
                if (user == null)
                    return Unauthorized(new { message = "Invalid email or password" });

                // Generate JWT token with dynamic session key
                var token = _jwtService.GenerateToken(user, loginDto.RememberMe);

                // Set the JWT token in a cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Set to true in production with HTTPS
                    SameSite = SameSiteMode.Strict,
                    // Set expiry based on "remember me" option
                    Expires = loginDto.RememberMe 
                        ? DateTimeOffset.UtcNow.AddDays(30) 
                        : DateTimeOffset.UtcNow.AddMinutes(30)
                };
                
                Response.Cookies.Append("auth_token", token, cookieOptions);

                // Return response with token and user data
                return Ok(new {
                    token,
                    tokenExpiry = loginDto.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(30),
                    rememberMe = loginDto.RememberMe,
                    user = new { id = user.Id, isAdmin = user.IsAdmin, /*…*/ },
                    redirectUrl = user.IsAdmin ? "/admin/dashboard" : "/user/home"
                });
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear the authentication cookie
            Response.Cookies.Delete("auth_token");
            
            return Ok(new { message = "Logged out successfully" });
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

        // Method to get the current authenticated user
        // Replace the placeholder with this implementation
        private Users GetCurrentUser()
        {
            // Get token from cookie
            var token = Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token))
                return null;

            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
                return null;

            // This is simplified; in a real implementation, you would:
            // 1. Get the user ID from claims
            // 2. Use _userService to fetch the complete user from the database
            
            // For now, we'll return a minimal user object based on claims
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = principal.FindFirst(ClaimTypes.Name)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var isAdmin = principal.IsInRole("Admin");
            
            if (userId == null)
                return null;
                
            return new Users
            {
                Id = Guid.Parse(userId),
                Username = username,
                Email = email,
                IsAdmin = isAdmin
            };
        }
    }
}
