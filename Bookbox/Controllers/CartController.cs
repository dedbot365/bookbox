using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public CartController(ICartService cartService, IBookService bookService, IDiscountService discountService)
        {
            _cartService = cartService;
            _bookService = bookService;
            _discountService = discountService;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
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
            
            return View(cartItems);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(Guid bookId, int quantity = 1, decimal price = 0, string returnUrl = null)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }
            
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var book = await _bookService.GetBookByIdAsync(bookId);
            
            if (book == null)
            {
                return NotFound();
            }
            
            // Check if requested quantity is available
            if (book.Stock < quantity)
            {
                TempData["ErrorMessage"] = "Requested quantity exceeds available stock.";
                return Redirect(returnUrl ?? Url.Action("Index", "Shop"));
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
            
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(Guid cartItemId, string returnUrl = null)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _cartService.RemoveCartItemAsync(userId, cartItemId);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Item removed from cart successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove item from cart.";
            }
            
            if (!string.IsNullOrEmpty(returnUrl))
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
            
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cartItems = await _cartService.GetUserCartItemsAsync(userId, count);
            
            var result = cartItems.Select(item => new
            {
                cartItemId = item.CartItemId,
                quantity = item.Quantity,
                book = new
                {
                    bookId = item.Book.BookId,
                    title = item.Book.Title,
                    author = item.Book.Author,
                    price = item.Book.Price,
                    stock = item.Book.Stock,
                    imageUrl = item.Book.ImageUrl,
                    isOnSale = item.Book.Discounts.Any(d => d.IsOnSale && 
                              d.StartDate <= DateTime.UtcNow && 
                              (d.EndDate == null || d.EndDate > DateTime.UtcNow)),
                    discountedPrice = item.Book.Discounts
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
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var order = await _cartService.GetOrderAsync(id);
            
            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }
            
            return View(order);
        }
    }

    public class UpdateQuantityRequest
    {
        public Guid CartItemId { get; set; }
        public int Change { get; set; }
    }
}