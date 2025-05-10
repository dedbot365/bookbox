using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookbox.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }
        
        // Show all orders for the current user
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return RedirectToAction("Login", "Account");
                }
                
                // Get user's orders
                var orders = await _orderService.GetUserOrdersAsync(userId);
                
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while retrieving your orders.";
                return RedirectToAction("Index", "Home");
            }
        }
        
        // Show details for a specific order
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return RedirectToAction("Login", "Account");
                }
                
                // Get order details
                var order = await _orderService.GetOrderByIdAsync(id);
                
                // Check if order exists and belongs to current user
                if (order == null || order.UserId != userId)
                {
                    return NotFound();
                }
                
                return View(order);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while retrieving order details.";
                return RedirectToAction("Index");
            }
        }
        
        // Cancel an order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return RedirectToAction("Login", "Account");
                }
                
                // Check if order can be cancelled
                var canCancel = await _orderService.CanOrderBeCancelledAsync(id, userId);
                if (!canCancel)
                {
                    TempData["ErrorMessage"] = "This order cannot be cancelled. Orders can only be cancelled within 24 hours of placement and must be in Pending status.";
                    return RedirectToAction("Details", new { id });
                }
                
                // Cancel the order
                var cancelled = await _orderService.CancelOrderAsync(id, userId);
                if (cancelled)
                {
                    TempData["SuccessMessage"] = "Your order has been cancelled successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred while cancelling your order.";
                }
                
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while cancelling your order.";
                return RedirectToAction("Details", new { id });
            }
        }
        
        // Display the success page after order creation
        public async Task<IActionResult> Success(Guid id)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return RedirectToAction("Login", "Account");
                }
                
                // Get order details
                var order = await _orderService.GetOrderByIdAsync(id);
                
                // Check if order exists and belongs to current user
                if (order == null || order.UserId != userId)
                {
                    return NotFound();
                }
                
                return View(order);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while retrieving order details.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}