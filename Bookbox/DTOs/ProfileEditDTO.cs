using Bookbox.Constants;
using System.ComponentModel.DataAnnotations;

namespace Bookbox.DTOs
{
    public class ProfileEditDTO
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; } = string.Empty;
        
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
        
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        public string? CurrentImageUrl { get; set; }
        
        // Store original values for comparison
        public string OriginalUsername { get; set; } = string.Empty;
        public string OriginalEmail { get; set; } = string.Empty;
        
        public IFormFile? ImageFile { get; set; }
        
        // Address information
        [Required(ErrorMessage = "Address line 1 is required")]
        [StringLength(200, ErrorMessage = "Address line 1 cannot exceed 200 characters")]
        [Display(Name = "Address Line 1")]
        public string Address1 { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Address line 2 cannot exceed 200 characters")]
        [Display(Name = "Address Line 2")]
        public string? Address2 { get; set; }
        
        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "State is required")]
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string State { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; } = string.Empty;
        
        // Address ID if it exists
        public Guid? AddressId { get; set; }
    }
}