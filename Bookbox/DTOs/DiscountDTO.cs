using System.ComponentModel.DataAnnotations;

namespace Bookbox.DTOs
{
    public class DiscountDTO
    {
        public Guid? DiscountId { get; set; }
        
        [Required(ErrorMessage = "Book is required")]
        public Guid BookId { get; set; }
        
        [Required(ErrorMessage = "Discount percentage is required")]
        [Range(1, 90, ErrorMessage = "Discount must be between 1% and 90%")]
        [Display(Name = "Discount (%)")]
        public decimal DiscountPercentage { get; set; }
        
        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "End Date (Optional)")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        
        [Display(Name = "Mark as 'On Sale'")]
        public bool IsOnSale { get; set; } = true;
        
        // For display purposes in select lists
        public string? BookTitle { get; set; }
    }
}