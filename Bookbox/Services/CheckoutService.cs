using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookbox.Constants;
using Bookbox.DTOs;
using Bookbox.Data;
using Microsoft.EntityFrameworkCore;
using Bookbox.Services.Interfaces;

namespace Bookbox.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        
        public CheckoutService(ApplicationDbContext context, ICartService cartService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }
        
        public async Task<CheckoutDTO> PrepareCheckoutFromCartAsync(Guid userId)
        {
            // Get user information
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
            
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }
            
            // Get cart items
            var cartItems = await _cartService.GetUserCartItemsAsync(userId);
            if (!cartItems.Any())
            {
                throw new ApplicationException("Cart is empty");
            }
            
            // Create checkout DTO
            var checkout = new CheckoutDTO
            {
                UserId = userId,
                Username = user.Username ?? string.Empty,
                Items = cartItems.Select(item => new CheckoutItemDTO
                {
                    BookId = item.BookId,
                    Title = item.Book?.Title ?? "Unknown Book",
                    Author = item.Book?.Author ?? "Unknown Author",
                    CoverImageUrl = item.Book?.ImageUrl ?? string.Empty,
                    Quantity = item.Quantity,
                    Price = item.Book?.Price ?? 0
                }).ToList()
            };
            
            // Check loyalty discount eligibility
            checkout.HasLoyaltyDiscount = await CheckLoyaltyDiscountEligibilityAsync(userId);
            
            return checkout;
        }
        
        public async Task<CheckoutDTO> PrepareCheckoutFromCartAsync(Guid userId, List<Guid> selectedItems = null)
        {
            // Get user information
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
            
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }
            
            // Get cart items
            var cartItems = await _cartService.GetUserCartItemsAsync(userId);
            
            // Filter by selected items if provided
            if (selectedItems != null && selectedItems.Any())
            {
                cartItems = cartItems.Where(ci => selectedItems.Contains(ci.CartItemId)).ToList();
            }
            
            if (!cartItems.Any())
            {
                throw new ApplicationException("No items selected for checkout");
            }
            
            // Create checkout DTO
            var checkout = new CheckoutDTO
            {
                UserId = userId,
                Username = user.Username ?? string.Empty,
                Items = cartItems.Select(item => new CheckoutItemDTO
                {
                    BookId = item.BookId,
                    Title = item.Book?.Title ?? "Unknown Book",
                    Author = item.Book?.Author ?? "Unknown Author",
                    CoverImageUrl = item.Book?.ImageUrl ?? string.Empty,
                    Quantity = item.Quantity,
                    Price = item.Book?.Price ?? 0
                }).ToList()
            };
            
            // Check loyalty discount eligibility
            checkout.HasLoyaltyDiscount = await CheckLoyaltyDiscountEligibilityAsync(userId);
            
            return checkout;
        }
        
        public async Task<bool> CheckLoyaltyDiscountEligibilityAsync(Guid userId)
        {
            // Count the number of completed orders
            var completedOrdersCount = await _context.Orders
                .CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Completed);
            
            // Check if we've reached a multiple of 10
            // If the count is divisible by 10, user is eligible
            return completedOrdersCount > 0 && completedOrdersCount % 10 == 0;
        }
        
        public string GenerateClaimCode()
        {
            // Generate a unique claim code (8 characters)
            return Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }
        
        public async Task<CheckoutDTO> ConfirmCheckoutAsync(
            CheckoutDTO checkoutData, 
            PaymentMethod paymentMethod, 
            string notes)
        {
            // Set the payment method and notes
            checkoutData.PaymentMethod = paymentMethod;
            checkoutData.Notes = notes;
            
            // Generate a claim code
            checkoutData.ClaimCode = GenerateClaimCode();
            
            // Note: We don't create an actual order yet as per requirements
            // This would be where we'd persist the order if we were creating one
            
            return checkoutData;
        }
    }
}