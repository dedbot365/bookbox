using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bookbox.Constants;

namespace Bookbox.Models
{
    public class Book
    {
        [Key]
        public Guid BookId { get; set; }
        
        [Required]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
        public string Author { get; set; } = string.Empty;

        [Required]
        public Genre Genre { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required]
        public Format Format { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Publisher name cannot exceed 100 characters")]
        public string Publisher { get; set; } = string.Empty;

        [Required]
        [StringLength(20, ErrorMessage = "ISBN cannot exceed 20 characters")]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int Stock { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Language name cannot exceed 50 characters")]
        public string Language { get; set; } = string.Empty;

        public string? Awards { get; set; }

        [Required]
        public int PhysicalStock { get; set; }

        public string? ImageUrl { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }

        // New properties
        [Required]
        public DateTime PublicationDate { get; set; }
        
        [Required]
        public DateTime ArrivalDate { get; set; } = DateTime.UtcNow;
        
        public bool IsComingSoon { get; set; } = false;
        
        public int SalesCount { get; set; } = 0;
        
        public bool IsDeleted { get; set; } = false;
        
        // Navigation properties
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    }
}