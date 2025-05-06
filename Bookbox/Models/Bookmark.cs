using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookbox.Models
{
    public class Bookmark
    {
        [Key]
        public Guid BookmarkId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        [Required]
        public Guid BookId { get; set; }
        
        [ForeignKey("BookId")]
        public Book? Book { get; set; }
        
        [Required]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}