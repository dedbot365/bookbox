using Bookbox.Models;
using Bookbox.Data;
using Bookbox.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> ManageUsers()
        {
            // Get all users except admins
            var users = await _context.Users
                .Where(u => u.UserType != UserType.Admin)
                .OrderBy(u => u.Username)
                .ToListAsync();
            
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRoleAjax([FromForm] string userId, [FromForm] int userType)
        {
            try
            {
                // Convert string to GUID
                if (!Guid.TryParse(userId, out Guid userGuid))
                {
                    return Json(new { success = false, message = "Invalid user ID format" });
                }
                
                // Convert int to UserType enum
                if (!Enum.IsDefined(typeof(UserType), userType))
                {
                    return Json(new { success = false, message = "Invalid role specified" });
                }
                
                var userTypeEnum = (UserType)userType;
                
                // Only allow changing between Member and Staff roles
                if (userTypeEnum != UserType.Member && userTypeEnum != UserType.Staff)
                {
                    return Json(new { success = false, message = "Invalid role specified" });
                }
                
                var user = await _context.Users.FindAsync(userGuid);
                
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }
                
                if (user.UserType == UserType.Admin)
                {
                    return Json(new { success = false, message = "Cannot change admin roles" });
                }
                
                // Store old role for message
                var oldRole = user.UserType.ToString();
                user.UserType = userTypeEnum;
                await _context.SaveChangesAsync();
                
                return Json(new { 
                    success = true, 
                    message = $"User {user.Username}'s role changed from {oldRole} to {userTypeEnum}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}