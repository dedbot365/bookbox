using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Bookbox.Constants;
using Bookbox.DTOs;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookbox.Controllers
{
    [Authorize] // Ensures only logged-in users can access checkout
    public class CheckoutController : Controller
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        
        public CheckoutController(ICheckoutService checkoutService, IUserService userService, IOrderService orderService)
        {
            _checkoutService = checkoutService ?? throw new ArgumentNullException(nameof(checkoutService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }
        
        /// <summary>
        /// Displays the checkout page with cart items and calculated totals
        /// </summary>
        /// <returns>Checkout view with checkout data</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Checkout") });
                }
                
                // Prepare checkout data from cart
                var checkoutData = await _checkoutService.PrepareCheckoutFromCartAsync(userId);
                
                return View(checkoutData);
            }
            catch (ApplicationException ex)
            {
                // Handle empty cart or other application exceptions
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                TempData["ErrorMessage"] = "An error occurred while preparing your checkout. Please try again.";
                return RedirectToAction("Index", "Cart");
            }
        }
        
        /// <summary>
        /// Processes the checkout confirmation
        /// </summary>
        /// <param name="paymentMethod">Selected payment method</param>
        /// <param name="notes">Optional order notes</param>
        /// <returns>Redirects to success page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(PaymentMethod paymentMethod, string notes = "")
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return RedirectToAction("Login", "Account");
                }
                
                // Prepare checkout data
                var checkoutData = await _checkoutService.PrepareCheckoutFromCartAsync(userId);
                
                // Confirm the checkout (this will generate a claim code)
                var confirmedCheckout = await _checkoutService.ConfirmCheckoutAsync(
                    checkoutData,
                    paymentMethod,
                    notes
                );
                
                // Create an order from the checkout data
                var order = await _orderService.CreateOrderFromCheckoutAsync(confirmedCheckout);
                
                // Redirect to the Order controller's Success action with the order ID
                return RedirectToAction("Success", "Order", new { id = order.OrderId });
            }
            catch (ApplicationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Checkout");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while confirming your checkout. Please try again.";
                return RedirectToAction("Index", "Checkout");
            }
        }
        
        /// <summary>
        /// Displays the checkout success page with order details and claim code
        /// </summary>
        /// <returns>Success view with checkout data</returns>
        public async Task<IActionResult> Success()
        {
            try
            {
                // Retrieve the order ID from TempData
                if (TempData["OrderId"] is not string orderIdString || !Guid.TryParse(orderIdString, out var orderId))
                {
                    return RedirectToAction("Index", "Home");
                }
                
                // Get order details
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                
                return View(order);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}