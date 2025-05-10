using Bookbox.DTOs;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Member")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IBookService _bookService;
        private readonly IDiscountService _discountService;
        private readonly Bookbox.Data.ApplicationDbContext _context;

        public CartController(
            ICartService cartService,
            IBookService bookService,
            IDiscountService discountService,
            Bookbox.Data.ApplicationDbContext context)
        {
            _cartService = cartService;
            _bookService = bookService;
            _discountService = discountService;
            _context = context;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(); // Or handle appropriately
            }
            
            var cartItems = await _cartService.GetUserCartItemsAsync(userId);
            
            decimal totalPrice = 0;
            foreach (var item in cartItems)
            {
                // Check if book has active discount and book is not null
                if (item.Book != null)
                {
                    var discount = item.Book.Discounts?.FirstOrDefault(d => 
                        d.IsOnSale && 
                        d.StartDate <= DateTime.UtcNow && 
                        (d.EndDate == null || d.EndDate > DateTime.UtcNow));
                    
                    if (discount != null)
                    {
                        decimal discountedPrice = _discountService.CalculateDiscountedPrice(
                            item.Book.Price, discount.DiscountPercentage);
                        
                        totalPrice += discountedPrice * item.Quantity;
                    }
                    else
                    {
                        totalPrice += item.Book.Price * item.Quantity;
                    }
                }
            }
            
            ViewBag.TotalPrice = totalPrice;
            
            return View(cartItems);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(Guid bookId, int quantity = 1, decimal price = 0, string? returnUrl = null)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }
            
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(); // Or handle appropriately
            }
            
            var book = await _bookService.GetBookByIdAsync(bookId);
            
            if (book == null)
            {
                return NotFound();
            }
            
            // Check if requested quantity is available
            if (book.Stock < quantity)
            {
                TempData["ErrorMessage"] = "Requested quantity exceeds available stock.";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Shop");
            }
            
            var result = await _cartService.AddToCartAsync(userId, bookId, quantity, price);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Item added to cart successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add item to cart.";
            }
            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(Guid cartItemId, string? returnUrl = null)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(); // Or handle appropriately
            }
            
            var result = await _cartService.RemoveCartItemAsync(userId, cartItemId);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Item removed from cart successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove item from cart.";
            }
            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequest request)
        {
            if (request == null)
                return Json(new { success = false });
            
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Json(new { success = false, error = "Invalid user identification" });
            }
            var result = await _cartService.UpdateCartItemQuantityAsync(userId, request.CartItemId, request.Change);
            
            if (result)
            {
                int cartCount = await _cartService.GetCartItemCountAsync(userId);
                return Json(new { success = true, cartCount });
            }
            
            return Json(new { success = false });
        }
        
        // GET: Cart/GetCartItems
        [HttpGet]
        public async Task<IActionResult> GetCartItems(int count = 5)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Json(new { success = false, error = "Invalid user identification" });
            }
            
            var cartItems = await _cartService.GetUserCartItemsAsync(userId, count);
            
            var result = cartItems.Select(item => new CartItemDTO
            {
                CartItemId = item.CartItemId,
                Quantity = item.Quantity,
                Book = new BookCartDTO
                {
                    BookId = item.Book.BookId,
                    Title = item.Book.Title,
                    Author = item.Book.Author,
                    Price = item.Book.Price,
                    Stock = item.Book.Stock,
                    ImageUrl = item.Book.ImageUrl,
                    IsOnSale = item.Book.Discounts.Any(d => d.IsOnSale && 
                              d.StartDate <= DateTime.UtcNow && 
                              (d.EndDate == null || d.EndDate > DateTime.UtcNow)),
                    DiscountedPrice = item.Book.Discounts
                        .Where(d => d.IsOnSale && d.StartDate <= DateTime.UtcNow && 
                               (d.EndDate == null || d.EndDate > DateTime.UtcNow))
                        .Select(d => _discountService.CalculateDiscountedPrice(
                            item.Book.Price, d.DiscountPercentage))
                        .FirstOrDefault()
                }
            });
            
            return Json(result);
        }
        
        // GET: Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(); // Or handle the error appropriately
            }
            var cartItems = await _cartService.GetUserCartItemsAsync(userId);
            
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }
            
            // Check if all items are in stock
            foreach (var item in cartItems)
            {
                if (item.Book.Stock < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Insufficient stock for {item.Book.Title}. Available: {item.Book.Stock}";
                    return RedirectToAction(nameof(Index));
                }
            }
            
            decimal totalPrice = 0;
            foreach (var item in cartItems)
            {
                // Check if book has active discount
                var discount = item.Book.Discounts.FirstOrDefault(d => 
                    d.IsOnSale && 
                    d.StartDate <= DateTime.UtcNow && 
                    (d.EndDate == null || d.EndDate > DateTime.UtcNow));
                
                if (discount != null)
                {
                    decimal discountedPrice = _discountService.CalculateDiscountedPrice(
                        item.Book.Price, discount.DiscountPercentage);
                    
                    totalPrice += discountedPrice * item.Quantity;
                }
                else
                {
                    totalPrice += item.Book.Price * item.Quantity;
                }
            }
            
            ViewBag.TotalPrice = totalPrice;
            ViewBag.CartItems = cartItems;
            
            return View();
        }
        
        // POST: Cart/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(); // Or handle appropriately
            }
            var cartItems = await _cartService.GetUserCartItemsAsync(userId);
            
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }
            
            // Check if all items are in stock
            foreach (var item in cartItems)
            {
                if (item.Book.Stock < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Insufficient stock for {item.Book.Title}. Available: {item.Book.Stock}";
                    return RedirectToAction(nameof(Index));
                }
            }
            
            try
            {
                var order = await _cartService.CreateOrderFromCartAsync(userId);
                if (order != null)
                {
                    TempData["SuccessMessage"] = "Order placed successfully! Order ID: " + order.OrderId;
                    return RedirectToAction("OrderConfirmation", new { id = order.OrderId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create order.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        
        // GET: Cart/OrderConfirmation/5
        public async Task<IActionResult> OrderConfirmation(Guid id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(); // Or handle appropriately
            }
            var order = await _cartService.GetOrderAsync(id);
            
            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }
            
            return View(order);
        }

        // GET: Cart/ViewCart
        public async Task<IActionResult> ViewCart()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cartItems = await _cartService.GetUserCartItemsAsync(userId);
            
            decimal totalPrice = 0;
            foreach (var item in cartItems)
            {
                // Check if book has active discount
                var discount = item.Book.Discounts.FirstOrDefault(d => 
                    d.IsOnSale && 
                    d.StartDate <= DateTime.UtcNow && 
                    (d.EndDate == null || d.EndDate > DateTime.UtcNow));
                
                if (discount != null)
                {
                    decimal discountedPrice = _discountService.CalculateDiscountedPrice(
                        item.Book.Price, discount.DiscountPercentage);
                    
                    totalPrice += discountedPrice * item.Quantity;
                }
                else
                {
                    totalPrice += item.Book.Price * item.Quantity;
                }
            }
            
            ViewBag.TotalPrice = totalPrice;
            ViewBag.SubtotalPrice = totalPrice;
            ViewBag.ShippingFee = 0; // You can adjust this based on your business logic
            
            // Get the user's address
            var userWithAddress = await _context.Users
                .Include(u => u.Addresses) // Changed from Address to Addresses (plural)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            
            ViewBag.UserAddress = userWithAddress?.Addresses.FirstOrDefault(); // Get the first address from collection
            
            return View(cartItems);
        }

        // Add this new endpoint to CartController.cs
        [HttpPost]
        public async Task<IActionResult> RemoveItemAjax([FromBody] UpdateQuantityRequest request)
        {
            if (request == null)
                return Json(new { success = false });
            
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _cartService.RemoveCartItemAsync(userId, request.CartItemId);
            
            if (result)
            {
                int cartCount = await _cartService.GetCartItemCountAsync(userId);
                return Json(new { success = true, cartCount });
            }
            
            return Json(new { success = false });
        }
    }
}