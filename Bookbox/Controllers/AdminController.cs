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
            var currentYear = DateTime.UtcNow.Year;
            var completedOrdersByMonth = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate.Year == currentYear)
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

            // Calculate monthly revenue
            var currentMonth = DateTime.UtcNow.Month;
            var currentUtcYear = DateTime.UtcNow.Year;
            var monthlyRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate.Month == currentMonth && o.OrderDate.Year == currentUtcYear)
                .SumAsync(o => o.TotalAmount);

            ViewBag.MonthlyRevenue = monthlyRevenue;

            // … inside Dashboard() after computing MonthlyRevenue …

            // --- Daily: last 7 days ---
            var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-6);
            var dailyOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate.Date >= sevenDaysAgo)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var dailyOrderCounts = new int[7];
            for (int i = 0; i < 7; i++)
            {
                var date = sevenDaysAgo.AddDays(i);
                dailyOrderCounts[i] = dailyOrders.FirstOrDefault(x => x.Date == date)?.Count ?? 0;
            }
            ViewBag.DailyOrderCounts = dailyOrderCounts;

            // --- Weekly: last 4 weeks (28 days) ---
            var fourWeeksAgo = DateTime.UtcNow.Date.AddDays(-28);
            var weeklyOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate.Date >= fourWeeksAgo)
                .GroupBy(o => ((o.OrderDate.Date - fourWeeksAgo).Days) / 7)
                .Select(g => new { Week = g.Key, Count = g.Count() })
                .ToListAsync();

            var weeklyOrderCounts = new int[4];
            for (int w = 0; w < 4; w++)
                weeklyOrderCounts[w] = weeklyOrders.FirstOrDefault(x => x.Week == w)?.Count ?? 0;
            ViewBag.WeeklyOrderCounts = weeklyOrderCounts;

            // --- Monthly: already in monthlyOrderCounts[12] …
            ViewBag.MonthlyOrderCounts = monthlyOrderCounts;

            // Get completed orders by year for the last 5 years
            var fiveYearsAgo = DateTime.UtcNow.Year - 4;
            var yearlyOrderCounts = new int[5];
            var completedOrdersByYear = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate.Year >= fiveYearsAgo)
                .GroupBy(o => o.OrderDate.Year)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .OrderBy(x => x.Year)
                .ToListAsync();

            foreach (var item in completedOrdersByYear)
            {
                var index = item.Year - fiveYearsAgo;
                if (index >= 0 && index < 5)
                {
                    yearlyOrderCounts[index] = item.Count;
                }
            }

            ViewBag.YearlyOrderCounts = yearlyOrderCounts;

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