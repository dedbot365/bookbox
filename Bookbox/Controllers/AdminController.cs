using Bookbox.Models;
using Bookbox.Data;
using Bookbox.Constants;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookService _bookService;
        private readonly IUserService _userService;
        private readonly IAnnouncementService _announcementService;

        public AdminController(
            ApplicationDbContext context,
            IBookService bookService,
            IUserService userService,
            IAnnouncementService announcementService)
        {
            _context = context;
            _bookService = bookService;
            _userService = userService;
            _announcementService = announcementService;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Get real data where available
            var books = await _bookService.GetAllBooksAsync();
            var users = await _userService.GetAllUsersAsync();
            var announcements = await _announcementService.GetAllAnnouncementsAsync();
            var recentAnnouncements = await _announcementService.GetRecentAnnouncementsAsync(5);

            // Calculate books by genre
            var booksByGenre = books
                .GroupBy(b => b.Genre)
                .Select(g => new { Genre = g.Key.ToString(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Calculate books by format 
            var booksByFormat = books
                .GroupBy(b => b.Format)
                .Select(g => new { Format = g.Key.ToString(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Pass data to view
            ViewBag.TotalBooks = books.Count;
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalAnnouncements = announcements.Count();
            ViewBag.Announcements = recentAnnouncements;
            ViewBag.BooksByGenre = booksByGenre;
            ViewBag.BooksByFormat = booksByFormat;

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

        [HttpGet]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            // Convert string to GUID
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return NotFound();
            }
            
            var user = await _context.Users
                .AsNoTracking() // For read-only operations
                .FirstOrDefaultAsync(u => u.UserId == userGuid);
            
            if (user == null)
            {
                return NotFound();
            }
            
            // Return the user data as JSON
            return Json(user);
        }
    }
}