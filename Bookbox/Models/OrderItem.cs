using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookbox.Models
{
    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; }
        
        [Required]
        public Guid OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
        
        [Required]
        public Guid BookId { get; set; }
        
        [ForeignKey("BookId")]
        public Book? Book { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountedPrice { get; set; }
    }
}