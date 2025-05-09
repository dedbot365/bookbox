using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookbox.DTOs;
using Bookbox.Models;
using Bookbox.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookbox.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartDto> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Create a new cart for the user if one doesn't exist
                cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return ConvertToCartDto(cart);
        }

        public async Task<CartDto> AddItemToCartAsync(Guid userId, Guid bookId, int quantity)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(); // Save the cart first
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.BookId == bookId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartItemId = Guid.NewGuid(),
                    CartId = cart.CartId,
                    BookId = bookId,
                    Quantity = quantity,
                    DateAdded = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem); // Add directly to context instead of navigation property
            }

            await _context.SaveChangesAsync();
            
            // Refresh cart to get updated items
            cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);
                
            return ConvertToCartDto(cart);
        }

        public async Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, int quantity)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                throw new Exception("Cart not found");

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
            if (cartItem == null)
                throw new Exception("Cart item not found");

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            await _context.SaveChangesAsync();
            return ConvertToCartDto(cart);
        }

        public async Task<CartDto> RemoveCartItemAsync(Guid userId, Guid cartItemId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                throw new Exception("Cart not found");

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
            if (cartItem == null)
                throw new Exception("Cart item not found");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            
            return ConvertToCartDto(cart);
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return false;

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();
            
            return true;
        }

        private CartDto ConvertToCartDto(Cart cart)
        {
            return new CartDto
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.CartItemId,
                    BookId = ci.BookId,
                    BookTitle = ci.Book?.Title,
                    BookAuthor = ci.Book?.Author,
                    BookPrice = ci.Book?.Price ?? 0,
                    BookImageUrl = ci.Book?.ImageUrl,  // Updated from CoverImageUrl to ImageUrl
                    Quantity = ci.Quantity,
                    DateAdded = ci.DateAdded
                }).ToList()
            };
        }
    }
}