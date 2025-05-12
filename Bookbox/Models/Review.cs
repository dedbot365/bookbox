using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookbox.Models
{
    public class Review
    {
        [Key]
        public Guid ReviewId { get; set; }
        
        [Required]
        public Guid BookId { get; set; }
        
        [ForeignKey("BookId")]
        public Book? Book { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        public string Comment { get; set; } = string.Empty;
        
        [Required]
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
        
        public Guid OrderId { get; set; }
        
        public Guid OrderItemId { get; set; }
        
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
        
        [ForeignKey("OrderItemId")]
        public OrderItem? OrderItem { get; set; }
    }
}