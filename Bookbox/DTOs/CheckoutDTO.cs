using System;
using System.Collections.Generic;
using System.Linq;
using Bookbox.Constants;
using Bookbox.Models;

namespace Bookbox.DTOs
{
    public class CheckoutDTO
    {
        // User information
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        
        // Cart items
        public List<CheckoutItemDTO> Items { get; set; } = new List<CheckoutItemDTO>();
        
        // Order summary
        public int TotalBooks => Items.Sum(item => item.Quantity);
        public decimal Subtotal => Items.Sum(item => item.TotalPrice);
        
        // Discount information
        public bool HasBulkDiscount => TotalBooks >= 5;
        public decimal BulkDiscountRate => HasBulkDiscount ? 0.05m : 0;
        public decimal BulkDiscountAmount => Subtotal * BulkDiscountRate;
        
        public bool HasLoyaltyDiscount { get; set; }
        public decimal LoyaltyDiscountRate => HasLoyaltyDiscount ? 0.10m : 0;
        public decimal LoyaltyDiscountAmount => Subtotal * LoyaltyDiscountRate;
        
        public decimal TotalDiscountAmount => BulkDiscountAmount + LoyaltyDiscountAmount;
        public decimal TotalDiscountRate => BulkDiscountRate + (HasLoyaltyDiscount ? LoyaltyDiscountRate : 0);
        
        // Final totals
        public decimal FinalTotal => Subtotal - TotalDiscountAmount;
        
        // Generated data
        public string ClaimCode { get; set; } = string.Empty;
        
        // Payment information (default for now)
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
        
        // Notes
        public string Notes { get; set; } = string.Empty;
    }
    
    public class CheckoutItemDTO
    {
        public Guid BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;
    }
}