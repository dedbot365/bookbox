using Bookbox.Data;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookbox.Constants;

namespace Bookbox.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDiscountService _discountService;

        public CartService(ApplicationDbContext context, IDiscountService discountService)
        {
            _context = context;
            _discountService = discountService;
        }

        public async Task<IEnumerable<CartItem>> GetUserCartItemsAsync(Guid userId, int count = 0)
        {
            // Create base query
            var query = _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Book)
                    .ThenInclude(b => b.Discounts)
                .Where(ci => ci.Cart.UserId == userId);
                
            // Create an ordered query
            var orderedQuery = query.OrderByDescending(ci => ci.DateAdded);
            
            // Apply limit if needed
            if (count > 0)
                return await orderedQuery.Take(count).ToListAsync();
                
            return await orderedQuery.ToListAsync();
        }

        public async Task<bool> AddToCartAsync(Guid userId, Guid bookId, int quantity = 1, decimal price = 0)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var book = await _context.Books.FindAsync(bookId);
            
            if (book == null || book.Stock < quantity)
                return false;
            
            // Check if book already exists in cart
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Cart.UserId == userId && ci.BookId == bookId);
            
            if (existingItem != null)
            {
                // Update quantity if the book is already in the cart
                if (book.Stock < existingItem.Quantity + quantity)
                    return false;
                
                existingItem.Quantity += quantity;
                existingItem.DateAdded = DateTime.UtcNow;
            }
            else
            {
                // Create new cart item
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    BookId = bookId,
                    Quantity = quantity,
                    DateAdded = DateTime.UtcNow
                };
                
                _context.CartItems.Add(cartItem);
            }
            
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCartItemQuantityAsync(Guid userId, Guid cartItemId, int change)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Book)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId);
            
            if (cartItem == null)
                return false;
            
            // Check if we're decreasing and if quantity would be 0 or less
            if (change < 0 && cartItem.Quantity + change <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                // Check if we're increasing and would exceed stock
                if (change > 0 && cartItem.Quantity + change > cartItem.Book.Stock)
                    return false;
                
                cartItem.Quantity += change;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveCartItemAsync(Guid userId, Guid cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId);
            
            if (cartItem == null)
                return false;
            
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCartItemCountAsync(Guid userId)
        {
            return await _context.CartItems
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId == userId)
                .SumAsync(ci => ci.Quantity);
        }

        public async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };
                
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            
            return cart;
        }

        public async Task<bool> IsInCartAsync(Guid userId, Guid bookId)
        {
            return await _context.CartItems
                .Include(ci => ci.Cart)
                .AnyAsync(ci => ci.Cart.UserId == userId && ci.BookId == bookId);
        }

        public async Task<Order> CreateOrderFromCartAsync(Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.Book)
                    .ThenInclude(b => b.Discounts)
                    .Where(ci => ci.Cart.UserId == userId)
                    .ToListAsync();
                
                if (!cartItems.Any())
                    return null;
                
                // Generate a unique claim code (6 characters)
                string claimCode = GenerateRandomCode();
                
                // Create new order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    ClaimCode = claimCode
                };
                
                decimal totalAmount = 0;
                decimal discountApplied = 0;
                
                foreach (var item in cartItems)
                {
                    // Check if book is still in stock
                    if (item.Book.Stock < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for {item.Book.Title}");
                    
                    // Calculate price considering any active discounts
                    var activeDiscount = item.Book.Discounts
                        .FirstOrDefault(d => d.IsOnSale && 
                            d.StartDate <= DateTime.UtcNow && 
                            (d.EndDate == null || d.EndDate > DateTime.UtcNow));
                    
                    decimal itemPrice = item.Book.Price;
                    decimal itemDiscountAmount = 0;
                    
                    if (activeDiscount != null)
                    {
                        decimal discountedPrice = _discountService.CalculateDiscountedPrice(
                            item.Book.Price, activeDiscount.DiscountPercentage);
                        
                        itemDiscountAmount = (item.Book.Price - discountedPrice) * item.Quantity;
                        itemPrice = discountedPrice;
                    }
                    
                    // Add item to order - Note: Changed UnitPrice to Price to match OrderItem model
                    var orderItem = new OrderItem
                    {
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        Price = itemPrice
                    };
                    
                    order.OrderItems.Add(orderItem);
                    
                    // Update book stock only, not SalesCount
                    item.Book.Stock -= item.Quantity;
                }
                
                order.TotalAmount = totalAmount;
                order.DiscountApplied = discountApplied;
                
                // Add order to database
                _context.Orders.Add(order);
                
                // Clear the cart
                await ClearCartAsync(userId);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Order> GetOrderAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId == userId)
                .ToListAsync();
            
            if (!cartItems.Any())
                return true;
            
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> RemoveCartItemsByBookIdsAsync(Guid userId, List<Guid> bookIds)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId == userId && bookIds.Contains(ci.BookId))
                .ToListAsync();
            
            if (!cartItems.Any())
                return false;
            
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            return true;
        }
        
        private string GenerateRandomCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}