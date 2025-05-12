using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Bookbox.Data;
using Bookbox.Models;
using Bookbox.Constants;
using Bookbox.Services.Interfaces;
using System;
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
        
        public IActionResult Dashboard()
        {
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