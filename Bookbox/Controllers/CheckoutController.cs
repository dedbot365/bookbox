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
        
        public CheckoutController(ICheckoutService checkoutService, IUserService userService)
        {
            _checkoutService = checkoutService ?? throw new ArgumentNullException(nameof(checkoutService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
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
                
                // Prepare checkout data again to ensure it's fresh
                var checkoutData = await _checkoutService.PrepareCheckoutFromCartAsync(userId);
                
                // Confirm the checkout (this will generate a claim code)
                var confirmedCheckout = await _checkoutService.ConfirmCheckoutAsync(
                    checkoutData,
                    paymentMethod,
                    notes
                );
                
                // Store the confirmed checkout in TempData for the success page
                TempData["CheckoutData"] = System.Text.Json.JsonSerializer.Serialize(confirmedCheckout);
                
                return RedirectToAction("Success");
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
        public IActionResult Success()
        {
            try
            {
                // Retrieve the checkout data from TempData
                if (TempData["CheckoutData"] is not string checkoutJson)
                {
                    return RedirectToAction("Index", "Home");
                }
                
                var checkout = System.Text.Json.JsonSerializer.Deserialize<CheckoutDTO>(checkoutJson);
                if (checkout == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                
                return View(checkout);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}