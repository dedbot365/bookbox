using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookbox.Models
{
    public class CartItem
    {
        [Key]
        public Guid CartItemId { get; set; }
        
        [Required]
        public Guid CartId { get; set; }
        
        [ForeignKey("CartId")]
        public Cart? Cart { get; set; }
        
        [Required]
        public Guid BookId { get; set; }
        
        [ForeignKey("BookId")]
        public Book? Book { get; set; }
        
        [Required]
        public int Quantity { get; set; } = 1;
        
        [Required]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}