using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookbox.Models
{
    public class Discount
    {
        [Key]
        public Guid DiscountId { get; set; }
        
        [Required]
        public Guid BookId { get; set; }
        
        [ForeignKey("BookId")]
        public Book? Book { get; set; }
        
        [Required]
        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndDate { get; set; }
        
        [Required]
        public bool IsOnSale { get; set; } = true;
    }
}