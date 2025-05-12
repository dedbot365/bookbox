using Bookbox.Constants;
using Bookbox.Data;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookbox.Services
{
    public class ChartService : IChartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookService _bookService;

        public ChartService(ApplicationDbContext context, IBookService bookService)
        {
            _context = context;
            _bookService = bookService;
        }

        public async Task<Dictionary<string, object>> GetBookStatisticsAsync()
        {
            var books = await _bookService.GetAllBooksAsync();
            var result = new Dictionary<string, object>();

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

            result["TotalBooks"] = books.Count;
            result["BooksByGenre"] = booksByGenre;
            result["BooksByFormat"] = booksByFormat;
            result["TopSellingBooks"] = topSellingBooks;
            
            return result;
        }

        public async Task<Dictionary<string, object>> GetOrderStatisticsAsync()
        {
            var result = new Dictionary<string, object>();
            
            // Total orders count
            var totalOrders = await _context.Orders.CountAsync();
            result["TotalOrders"] = totalOrders;
            
            return result;
        }
        
        public async Task<Dictionary<string, object>> GetReviewStatisticsAsync()
        {
            var result = new Dictionary<string, object>();
            
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
            
            result["ReviewPercentages"] = reviewPercentages;
            return result;
        }
        
        public async Task<Dictionary<string, object>> GetTimeBasedOrderStatisticsAsync()
        {
            var result = new Dictionary<string, object>();
            
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
            result["DailyOrderCounts"] = dailyOrderCounts;

            // --- Weekly: last 4 weeks (28 days) ---
            var fourWeeksAgo = DateTime.UtcNow.Date.AddDays(-28);
            var weeklyOrderCounts = new int[4];

            // Get all completed orders from the past 4 weeks
            var weeklyCompletedOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate >= fourWeeksAgo)
                .ToListAsync();

            // Group them manually into weeks
            foreach (var order in weeklyCompletedOrders)
            {
                // Calculate which week this order belongs to (0-3)
                int daysSince = (order.OrderDate.Date - fourWeeksAgo.Date).Days;
                int weekIndex = Math.Min(daysSince / 7, 3); // Ensure it stays within bounds
                
                if (weekIndex >= 0 && weekIndex < 4)
                {
                    weeklyOrderCounts[weekIndex]++;
                }
            }
            result["WeeklyOrderCounts"] = weeklyOrderCounts;

            // --- Monthly: orders by month for the current year ---
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
            result["MonthlyOrderCounts"] = monthlyOrderCounts;

            // --- Yearly: last 5 years ---
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
            result["YearlyOrderCounts"] = yearlyOrderCounts;
            
            return result;
        }
        
        public async Task<object> GetRecentCompletedOrdersAsync(int count = 5)
        {
            // Get recent completed orders
            var recentCompletedOrders = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.Status == OrderStatus.Completed)
                .OrderByDescending(o => o.CompletedDate)
                .Take(count)
                .Select(o => new
                {
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User.FirstName + " " + o.User.LastName,
                    Amount = o.TotalAmount,
                    CompletedDate = o.CompletedDate ?? o.OrderDate,
                    Status = o.Status
                })
                .ToListAsync();
                
            return recentCompletedOrders;
        }
        
        public async Task<decimal> GetMonthlyRevenueAsync()
        {
            // Calculate monthly revenue
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var monthlyRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && 
                       o.OrderDate.Month == currentMonth && 
                       o.OrderDate.Year == currentYear)
                .SumAsync(o => o.TotalAmount);
                
            return monthlyRevenue;
        }
    }
}