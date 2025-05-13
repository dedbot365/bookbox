using Bookbox.Models;
using Bookbox.Data;
using Bookbox.Constants;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IChartService _chartService;

        public MemberController(
            ApplicationDbContext context,
            IChartService chartService)
        {
            _context = context;
            _chartService = chartService;
        }

        public async Task<IActionResult> Index()
        {
            // Get current user ID
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Get user's information
            var user = await _context.Users.FindAsync(userId);
            ViewBag.UserName = $"{user.FirstName} {user.LastName}";
            
            // Order statistics for this member
            var completedOrders = await _context.Orders
                .CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Completed);
                
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Pending);
                
            var cancelledOrders = await _context.Orders
                .CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Cancelled);
                
            var totalReviews = await _context.Reviews
                .CountAsync(r => r.UserId == userId);
            
            // Expense calculations
            var today = DateTime.UtcNow.Date;
            var dailyExpense = await _context.Orders
                .Where(o => o.UserId == userId && 
                      o.Status == OrderStatus.Completed && 
                      o.OrderDate.Date == today)
                .SumAsync(o => o.TotalAmount);
            
            var weekStart = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek), DateTimeKind.Utc);
            var weeklyExpense = await _context.Orders
                .Where(o => o.UserId == userId && 
                      o.Status == OrderStatus.Completed && 
                      o.OrderDate >= weekStart)
                .SumAsync(o => o.TotalAmount);
            
            // FIXED: Specify DateTimeKind.Utc for new DateTime objects
            var monthStart = DateTime.SpecifyKind(
                new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), 
                DateTimeKind.Utc);
            var monthlyExpense = await _context.Orders
                .Where(o => o.UserId == userId && 
                      o.Status == OrderStatus.Completed && 
                      o.OrderDate >= monthStart)
                .SumAsync(o => o.TotalAmount);
                
            var totalExpense = await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);
            
            // Get recent orders for this member
            var recentOrders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    Date = o.OrderDate,
                    CompletedDate = o.CompletedDate,
                    Amount = o.TotalAmount,
                    Status = o.Status
                })
                .ToListAsync();
            
            // Monthly order data for charts
            var currentYear = DateTime.UtcNow.Year;
            var monthlyOrderCounts = new int[12];
            var monthlyExpenseData = new decimal[12];
            
            // Get all orders for order count chart
            var allOrdersByMonth = await _context.Orders
                .Where(o => o.UserId == userId && o.OrderDate.Year == currentYear)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();
                
            // Get only completed orders for expense chart
            var completedOrdersByMonth = await _context.Orders
                .Where(o => o.UserId == userId && 
                       o.OrderDate.Year == currentYear && 
                       o.Status == OrderStatus.Completed)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(o => o.TotalAmount) })
                .ToListAsync();
            
            // Fill in the monthly orders data
            foreach (var item in allOrdersByMonth)
            {
                monthlyOrderCounts[item.Month - 1] = item.Count;
            }
            
            // Fill in the monthly expense data (only from completed orders)
            foreach (var item in completedOrdersByMonth)
            {
                monthlyExpenseData[item.Month - 1] = item.Total;
            }
            
            // Get books by genre for this member
            var booksByGenre = await _context.OrderItems
                .Where(oi => oi.Order.UserId == userId && oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => oi.Book.Genre)
                .Select(g => new { genre = g.Key.ToString(), count = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.count)
                .ToListAsync();
                
            // Get books by format for this member
            var booksByFormat = await _context.OrderItems
                .Where(oi => oi.Order.UserId == userId && oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => oi.Book.Format)
                .Select(g => new { format = g.Key.ToString(), count = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.count)
                .ToListAsync();
                
            // Order status distribution
            var orderStatusData = new List<object>
            {
                new { label = "Completed", count = completedOrders, color = "#1cc88a" },
                new { label = "Pending", count = pendingOrders, color = "#f6c23e" },
                new { label = "Cancelled", count = cancelledOrders, color = "#e74a3b" }
            };
            
            // Pass data to view
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.CancelledOrders = cancelledOrders;
            ViewBag.TotalReviews = totalReviews;
            
            ViewBag.DailyExpense = dailyExpense;
            ViewBag.WeeklyExpense = weeklyExpense;
            ViewBag.MonthlyExpense = monthlyExpense;
            ViewBag.TotalExpense = totalExpense;
            
            ViewBag.RecentOrders = recentOrders;
            ViewBag.MonthlyOrderCounts = monthlyOrderCounts;
            ViewBag.MonthlyExpenseData = monthlyExpenseData;
            
            ViewBag.BooksByGenre = booksByGenre;
            ViewBag.BooksByFormat = booksByFormat;
            ViewBag.OrderStatusData = orderStatusData;
            
            return View();
        }
    }
}