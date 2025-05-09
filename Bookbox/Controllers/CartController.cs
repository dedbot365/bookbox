using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Bookbox.Services;
using Bookbox.DTOs;

namespace Bookbox.Controllers
{
    [Authorize] // Ensures only authenticated users can access
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(Guid bookId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _cartService.AddItemToCartAsync(userId, bookId, quantity);
                TempData["SuccessMessage"] = "Item added to your cart successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding item to cart: {ex.Message}";
            }

            // Return to the referring page or shop page
            if (Request.Headers["Referer"].ToString().Contains("/Book/Details"))
            {
                return RedirectToAction("Details", "Book", new { id = bookId });
            }
            return RedirectToAction("Index", "Shop");
        }

        // POST: Cart/UpdateItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItem(Guid cartItemId, int quantity)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _cartService.UpdateCartItemAsync(userId, cartItemId, quantity);
                TempData["SuccessMessage"] = "Cart updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating cart: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(Guid cartItemId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _cartService.RemoveCartItemAsync(userId, cartItemId);
                TempData["SuccessMessage"] = "Item removed from cart!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error removing item: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/ClearCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _cartService.ClearCartAsync(userId);
                TempData["SuccessMessage"] = "Your cart has been cleared!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error clearing cart: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}