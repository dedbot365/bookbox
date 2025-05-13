using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Bookbox.Data;
using Bookbox.Models;
using Bookbox.Constants;
using Bookbox.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderEmailService _orderEmailService;
        private readonly IOrderService _orderService;
        private readonly IChartService _chartService; 

        public StaffController(
            ApplicationDbContext context, 
            IOrderEmailService orderEmailService, 
            IOrderService orderService,
            IChartService chartService) 
        {
            _context = context;
            _orderEmailService = orderEmailService;
            _orderService = orderService;
            _chartService = chartService; 
        }
        
        public async Task<IActionResult> Dashboard()
        {
            // Get chart data from service (similar to AdminController)
            var bookStats = await _chartService.GetBookStatisticsAsync();
            var orderStats = await _chartService.GetOrderStatisticsAsync();
            var timeBasedStats = await _chartService.GetTimeBasedOrderStatisticsAsync();
            
            // Get revenue data (this was missing)
            var monthlyRevenue = await _chartService.GetMonthlyRevenueAsync();
            var totalRevenue = await _chartService.GetTotalRevenueAsync();
            var weeklyRevenue = await _chartService.GetWeeklyRevenueAsync();
            var dailyRevenue = await _chartService.GetDailyRevenueAsync();
            
            // Get time-based revenue data (this was missing)
            var timeBasedRevenueStats = await _chartService.GetTimeBasedRevenueStatisticsAsync();
            
            // Get order status counts
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending);
            var completedOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Completed);
            var cancelledOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Cancelled);
            
            // Pass data to view (including the previously missing revenue data)
            ViewBag.TotalBooks = bookStats["TotalBooks"];
            ViewBag.TotalOrders = orderStats["TotalOrders"];
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.CancelledOrders = cancelledOrders;
            ViewBag.BooksByGenre = bookStats["BooksByGenre"];
            ViewBag.BooksByFormat = bookStats["BooksByFormat"];
            ViewBag.TopSellingBooks = bookStats["TopSellingBooks"];
            
            // Add time-based order data (some of these were missing)
            ViewBag.MonthlyOrderCounts = timeBasedStats["MonthlyOrderCounts"];
            ViewBag.DailyOrderCounts = timeBasedStats["DailyOrderCounts"];
            ViewBag.WeeklyOrderCounts = timeBasedStats["WeeklyOrderCounts"];
            ViewBag.YearlyOrderCounts = timeBasedStats["YearlyOrderCounts"];
            
            // Add revenue data for cards (these were missing or incorrect)
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.WeeklyRevenue = weeklyRevenue;
            ViewBag.DailyRevenue = dailyRevenue;
            
            // Add time-based revenue data for charts (these were missing)
            ViewBag.DailyRevenueData = timeBasedRevenueStats["DailyRevenueData"];
            ViewBag.WeeklyRevenueData = timeBasedRevenueStats["WeeklyRevenueData"];
            ViewBag.MonthlyRevenueData = timeBasedRevenueStats["MonthlyRevenueData"];
            ViewBag.YearlyRevenueData = timeBasedRevenueStats["YearlyRevenueData"];
            
            // Get recent orders for the table
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User.FirstName + " " + o.User.LastName,
                    Amount = o.TotalAmount,
                    Status = o.Status,
                    OrderDate = o.OrderDate
                })
                .ToListAsync();
            ViewBag.RecentOrders = recentOrders;
            
            return View();
        }
        
        public async Task<IActionResult> Orders(string searchTerm = "", string sortBy = "date_desc", string statusFilter = "")
        {
            var query = _context.Orders.Include(o => o.User);
            // Start with all orders query
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .AsQueryable();
            
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Convert OrderNumber to string before using Contains
                ordersQuery = ordersQuery.Where(o => o.OrderNumber.ToString().Contains(searchTerm));
                ViewData["SearchTerm"] = searchTerm;
            }
            
            // Apply status filter if provided
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                if (Enum.TryParse<OrderStatus>(statusFilter, true, out var status))
                {
                    ordersQuery = ordersQuery.Where(o => o.Status == status);
                }
                ViewData["StatusFilter"] = statusFilter;
            }
            
            // Apply sorting
            ordersQuery = sortBy switch
            {
                "date_asc" => ordersQuery.OrderBy(o => o.OrderDate),
                "date_desc" => ordersQuery.OrderByDescending(o => o.OrderDate),
                "total_asc" => ordersQuery.OrderBy(o => o.TotalAmount),
                "total_desc" => ordersQuery.OrderByDescending(o => o.TotalAmount),
                "customer_asc" => ordersQuery.OrderBy(o => o.User.FirstName).ThenBy(o => o.User.LastName),
                "order_asc" => ordersQuery.OrderBy(o => o.OrderNumber),
                "order_desc" => ordersQuery.OrderByDescending(o => o.OrderNumber),
                _ => ordersQuery.OrderByDescending(o => o.OrderDate) // default sorting
            };
            var orders = await ordersQuery.ToListAsync();
            ViewData["CurrentSort"] = sortBy;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["SearchTerm"] = searchTerm;
            
            
            return View(orders);
        }
        
        public async Task<IActionResult> OrderDetails(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id);
                
            if (order == null)
            {
                return NotFound();
            }
            
            return View(order);
        }
        
        [HttpPost]
        public async Task<IActionResult> RedeemOrder(Guid id, string claimCode)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            
            // Check if request is AJAX
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            if (order == null)
            {
                return isAjax 
                    ? Json(new { success = false, message = "Order not found." })
                    : NotFound();
            }
            
            // Verify claim code
            if (order.ClaimCode != claimCode)
            {
                TempData["Error"] = "Invalid claim code. Please check and try again.";
                return isAjax
                    ? Json(new { success = false, message = "Invalid claim code. Please check and try again." })
                    : RedirectToAction(nameof(OrderDetails), new { id });
            }
            
            // Can only redeem pending orders
            if (order.Status != OrderStatus.Pending)
            {
                string errorMessage = order.Status == OrderStatus.Completed
                    ? "This order has already been completed and cannot be redeemed again."
                    : "This order was cancelled and cannot be redeemed.";
                
                TempData["Error"] = errorMessage;
                return isAjax
                    ? Json(new { success = false, message = errorMessage })
                    : RedirectToAction(nameof(OrderDetails), new { id });
            }
            
            // Use the OrderService to update the status, which will also update SalesCount
            await _orderService.UpdateOrderStatusAsync(id, OrderStatus.Completed);
            
            // Record the staff member who processed it
            var staffUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(staffUserId, out Guid staffId))
            {
                order.ProcessedBy = staffId;
                await _context.SaveChangesAsync(); // Save this change separately
            }
            
            // Update book stock based on order items
            foreach (var item in order.OrderItems)
            {
                if (item.Book != null)
                {
                    // Ensure stock doesn't go below zero
                    item.Book.Stock = Math.Max(0, item.Book.Stock - item.Quantity);
                    // Update physical stock too if needed
                    item.Book.PhysicalStock = Math.Max(0, item.Book.PhysicalStock - item.Quantity);
                }
            }
            
            await _context.SaveChangesAsync();
            
            // Increase the user's successful order count
            if (order.UserId != Guid.Empty)
            {
                var user = await _context.Users.FindAsync(order.UserId);
                if (user != null)
                {
                    user.SuccessfulOrderCount++;
                    await _context.SaveChangesAsync();
                }
            }
            
            // Send order processed emails
            await _orderEmailService.SendOrderProcessedEmailsAsync(id, staffId);
            
            TempData["Success"] = "Order has been redeemed successfully!";
            
            return isAjax
                ? Json(new { success = true, message = "Order has been redeemed successfully!" })
                : RedirectToAction(nameof(Orders));
        }
    }
}