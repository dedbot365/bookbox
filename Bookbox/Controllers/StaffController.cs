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

        public StaffController(ApplicationDbContext context, IOrderEmailService orderEmailService, IOrderService orderService)
        {
            _context = context;
            _orderEmailService = orderEmailService;
            _orderService = orderService;
        }
        
        public async Task<IActionResult> Dashboard()
        {
            // Get books data
            var books = await _context.Books.ToListAsync();
            ViewBag.TotalBooks = books.Count;
            
            // Get books by genre
            var booksByGenre = books
                .GroupBy(b => b.Genre)
                .Select(g => new { genre = g.Key.ToString(), count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToList();
            ViewBag.BooksByGenre = booksByGenre;
            
            // Get books by format
            var booksByFormat = books
                .GroupBy(b => b.Format)
                .Select(g => new { format = g.Key.ToString(), count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToList();
            ViewBag.BooksByFormat = booksByFormat;
            
            // Get top selling books
            var topSellingBooks = books
                .OrderByDescending(b => b.SalesCount)
                .Take(5)
                .Select(b => new { title = b.Title, salesCount = b.SalesCount })
                .ToList();
            ViewBag.TopSellingBooks = topSellingBooks;
            
            // Get order statistics
            var totalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalOrders = totalOrders;
            
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending);
            ViewBag.PendingOrders = pendingOrders;
            
            var completedOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Completed);
            ViewBag.CompletedOrders = completedOrders;
            
            var cancelledOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Cancelled);
            ViewBag.CancelledOrders = cancelledOrders;
            
            // Calculate monthly revenue
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var monthlyRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && 
                       o.OrderDate.Month == currentMonth && 
                       o.OrderDate.Year == currentYear)
                .SumAsync(o => o.TotalAmount);
            ViewBag.MonthlyRevenue = monthlyRevenue;
            
            // Calculate total revenue (all completed orders)
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);
            ViewBag.TotalRevenue = totalRevenue;
            
            // Get monthly order counts for the line chart
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
            ViewBag.MonthlyOrderCounts = monthlyOrderCounts;
            
            // Get recent orders
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
        
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
                
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
            
            if (order == null)
            {
                return NotFound();
            }
            
            // Verify claim code
            if (order.ClaimCode != claimCode)
            {
                TempData["Error"] = "Invalid claim code. Please check and try again.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }
            
            // Can only redeem pending orders
            if (order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = order.Status == OrderStatus.Completed
                    ? "This order has already been completed and cannot be redeemed again."
                    : "This order was cancelled and cannot be redeemed.";
                return RedirectToAction(nameof(OrderDetails), new { id });
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
            return RedirectToAction(nameof(Orders));
        }
    }
}