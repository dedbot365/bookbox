using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookbox.Models
{
    public class Announcement
    {
        [Key]
        public Guid AnnouncementId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndDate { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        // Foreign key to User (Admin)
        [Required]
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        // Track when the announcement was last modified
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}