using System;
using System.Collections.Generic;
using System.Linq;
using Bookbox.Constants;
using Bookbox.Models;

namespace Bookbox.DTOs
{
    public class OrderDTO
    {
        public Guid OrderId { get; set; }
        public int OrderNumber { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountApplied { get; set; }
        public decimal Subtotal => TotalAmount + DiscountApplied;
        public string ClaimCode { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime? CompletedDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
        
        // Helper methods
        public bool CanBeCancelled 
        { 
            get 
            {
                return Status == OrderStatus.Pending && 
                       (DateTime.UtcNow - OrderDate).TotalHours <= 24;
            }
        }
        
        public TimeSpan TimeRemainingForCancellation
        {
            get
            {
                var deadline = OrderDate.AddHours(24);
                var timeLeft = deadline - DateTime.UtcNow;
                return timeLeft.TotalSeconds > 0 ? timeLeft : TimeSpan.Zero;
            }
        }
    }
    
    public class OrderItemDTO
    {
        public Guid OrderItemId { get; set; }
        public Guid OrderId { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal Subtotal => Quantity * (DiscountedPrice > 0 ? DiscountedPrice : Price);
    }
}