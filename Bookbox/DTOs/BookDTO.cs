using Bookbox.Constants;
using System.ComponentModel.DataAnnotations;

namespace Bookbox.DTOs
{
    public class BookDTO
    {
        [Required]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required]
        public Genre Genre { get; set; }

        [Required]
        [Range(0, 9999.99)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(50)]
        public string Format { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Publisher { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        [StringLength(50)]
        public string Language { get; set; } = string.Empty;

        public string? Awards { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int PhysicalStock { get; set; }

        public IFormFile? ImageFile { get; set; }
        
        // UserId will be set from the current authenticated user
        public Guid? UserId { get; set; }
    }
}