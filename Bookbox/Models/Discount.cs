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
        [Range(1, 90, ErrorMessage = "Discount percentage must be between 1 and 90")]
        public decimal DiscountPercentage { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Required]
        public bool IsOnSale { get; set; } = true;
    }
}