using System;
using System.ComponentModel.DataAnnotations;

namespace Bookbox.DTOs
{
    public class ReviewDTO
    {
        public Guid ReviewId { get; set; }
        
        public Guid BookId { get; set; }
        
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
        
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; } = string.Empty;
        
        public DateTime ReviewDate { get; set; }
        
        public string UserName { get; set; } = string.Empty;
        
        public Guid OrderId { get; set; }
        
        public Guid OrderItemId { get; set; }
    }
}