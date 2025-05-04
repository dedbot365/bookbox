using System;
using System.Threading.Tasks;
using bookbox.Entities;
using bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace bookbox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] Users user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdUser = await _userService.CreateUserAsync(user);
            return CreatedAtAction(nameof(CreateUser), new { id = createdUser.Id }, createdUser);
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
