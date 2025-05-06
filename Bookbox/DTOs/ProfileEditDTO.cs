using Bookbox.Constants;
using System.ComponentModel.DataAnnotations;

namespace Bookbox.DTOs
{
    public class ProfileEditDTO
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string? ContactNo { get; set; }

        public Gender Gender { get; set; }

        public string? CurrentImageUrl { get; set; }
        
        public IFormFile? ImageFile { get; set; }
    }
}