using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Bookbox.Data;
using Bookbox.Models;
using Bookbox.Constants;
using System.Security.Claims;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
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
            var order = await _context.Orders.FindAsync(id);
            
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
            
            // Update order status
            order.Status = OrderStatus.Completed;
            order.CompletedDate = DateTime.UtcNow;
            
            // Record the staff member who processed it
            var staffUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(staffUserId, out Guid staffId))
            {
                order.ProcessedBy = staffId;
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
            
            TempData["Success"] = "Order has been redeemed successfully!";
            return RedirectToAction(nameof(Orders));
        }
    }
}