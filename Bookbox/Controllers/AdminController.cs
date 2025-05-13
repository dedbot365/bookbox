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
        private readonly IChartService _chartService;

        public AdminController(
            ApplicationDbContext context,
            IBookService bookService,
            IUserService userService,
            IAnnouncementService announcementService,
            IChartService chartService)
        {
            _context = context;
            _bookService = bookService;
            _userService = userService;
            _announcementService = announcementService;
            _chartService = chartService;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Get user and announcement data
            var users = await _userService.GetAllUsersAsync();
            var announcements = await _announcementService.GetAllAnnouncementsAsync();
            var recentAnnouncements = await _announcementService.GetRecentAnnouncementsAsync(5);
            
            // Get chart data from service
            var bookStats = await _chartService.GetBookStatisticsAsync();
            var orderStats = await _chartService.GetOrderStatisticsAsync();
            var reviewStats = await _chartService.GetReviewStatisticsAsync();
            var timeBasedStats = await _chartService.GetTimeBasedOrderStatisticsAsync();
            var recentCompletedOrders = await _chartService.GetRecentCompletedOrdersAsync();
            var monthlyRevenue = await _chartService.GetMonthlyRevenueAsync();
            var totalRevenue = await _chartService.GetTotalRevenueAsync();
            var weeklyRevenue = await _chartService.GetWeeklyRevenueAsync();
            var dailyRevenue = await _chartService.GetDailyRevenueAsync();

            // Get time-based revenue data
            var timeBasedRevenueStats = await _chartService.GetTimeBasedRevenueStatisticsAsync();

            // Pass data to view
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalBooks = bookStats["TotalBooks"];
            ViewBag.TotalAnnouncements = announcements.Count();
            ViewBag.TotalOrders = orderStats["TotalOrders"];
            ViewBag.Announcements = recentAnnouncements;
            ViewBag.BooksByGenre = bookStats["BooksByGenre"];
            ViewBag.BooksByFormat = bookStats["BooksByFormat"];
            ViewBag.TopSellingBooks = bookStats["TopSellingBooks"];
            ViewBag.MonthlyOrderCounts = timeBasedStats["MonthlyOrderCounts"];
            ViewBag.DailyOrderCounts = timeBasedStats["DailyOrderCounts"];
            ViewBag.WeeklyOrderCounts = timeBasedStats["WeeklyOrderCounts"];
            ViewBag.YearlyOrderCounts = timeBasedStats["YearlyOrderCounts"];
            ViewBag.ReviewPercentages = reviewStats["ReviewPercentages"];
            ViewBag.RecentCompletedOrders = recentCompletedOrders;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.WeeklyRevenue = weeklyRevenue;
            ViewBag.DailyRevenue = dailyRevenue;

            ViewBag.DailyRevenueData = timeBasedRevenueStats["DailyRevenueData"];
            ViewBag.WeeklyRevenueData = timeBasedRevenueStats["WeeklyRevenueData"];
            ViewBag.MonthlyRevenueData = timeBasedRevenueStats["MonthlyRevenueData"];
            ViewBag.YearlyRevenueData = timeBasedRevenueStats["YearlyRevenueData"];

            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUsers(string searchTerm = "", int page = 1)
        {
            // Set page size
            int pageSize = 15;
            
            // Get all users except admins
            var query = _context.Users
                .Where(u => u.UserType != UserType.Admin)
                .AsQueryable();
            
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => 
                    u.FirstName.ToLower().Contains(searchTerm) || 
                    u.LastName.ToLower().Contains(searchTerm) || 
                    u.Username.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm));
                    
                ViewBag.SearchTerm = searchTerm;
            }
            
            // Get total count for pagination
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (float)pageSize);
            
            // Ensure valid page number
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            
            // Apply sorting and pagination
            var users = await query
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Set pagination data for the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            
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