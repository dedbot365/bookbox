using System;
using System.ComponentModel.DataAnnotations;

namespace Bookbox.DTOs
{
    public class AnnouncementDto
    {
        public Guid? AnnouncementId { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Content is required")]
        [Display(Name = "Description")]
        public string Content { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public Guid? UserId { get; set; }
        
        public string? UserName { get; set; }
        
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}