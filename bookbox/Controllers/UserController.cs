using System;
using bookbox.Entities;
using bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    }
}
