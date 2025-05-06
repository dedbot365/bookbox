using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bookbox.Constants;

namespace Bookbox.Models
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountApplied { get; set; } = 0;
        
        [Required]
        [StringLength(10)]
        public string ClaimCode { get; set; } = string.Empty;
        
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        public DateTime? CompletedDate { get; set; }
        
        // Navigation property
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
    
    // Add this to Bookbox.Constants namespace
    
}