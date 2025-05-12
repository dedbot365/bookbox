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
                
            // Get top selling books
            var topSellingBooks = books
                .OrderByDescending(b => b.SalesCount)
                .Take(5)
                .Select(b => new { Title = b.Title, SalesCount = b.SalesCount })
                .ToList();
                
            // Get completed orders by month for the current year
            var completedOrdersByMonth = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate.Year == DateTime.Now.Year)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(x => x.Month)
                .ToListAsync();

            var monthlyOrderCounts = new int[12];
            foreach (var item in completedOrdersByMonth)
            {
                monthlyOrderCounts[item.Month - 1] = item.Count;
            }
            
            // Get review statistics
            var reviewStats = await _context.Reviews
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalReviews = reviewStats.Sum(r => r.Count);
            var reviewPercentages = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                var count = reviewStats.FirstOrDefault(r => r.Rating == i)?.Count ?? 0;
                reviewPercentages[i] = totalReviews > 0 ? (int)Math.Round((double)count / totalReviews * 100) : 0;
            }
            
            // Get recent completed orders
            var recentCompletedOrders = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.Status == OrderStatus.Completed)
                .OrderByDescending(o => o.CompletedDate)
                .Take(5)
                .Select(o => new
                {
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User.FirstName + " " + o.User.LastName,
                    Amount = o.TotalAmount,
                    CompletedDate = o.CompletedDate ?? o.OrderDate,
                    Status = o.Status
                })
                .ToListAsync();

            // Total orders count
            var totalOrders = await _context.Orders.CountAsync();

            // Pass data to view
            ViewBag.TotalBooks = books.Count;
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalAnnouncements = announcements.Count();
            ViewBag.TotalOrders = totalOrders;
            ViewBag.Announcements = recentAnnouncements;
            ViewBag.BooksByGenre = booksByGenre;
            ViewBag.BooksByFormat = booksByFormat;
            ViewBag.TopSellingBooks = topSellingBooks;
            ViewBag.MonthlyOrderCounts = monthlyOrderCounts;
            ViewBag.ReviewPercentages = reviewPercentages;
            ViewBag.RecentCompletedOrders = recentCompletedOrders;

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