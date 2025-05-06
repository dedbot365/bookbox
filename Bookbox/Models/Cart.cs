using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookbox.Models
{
    public class Cart
    {
        [Key]
        public Guid CartId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        // Navigation property
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}