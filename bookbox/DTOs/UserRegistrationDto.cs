using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace bookbox.DTOs
{
    public class UserRegistrationDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string Surname { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can only contain letters, numbers, periods, underscores, and hyphens.")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile ProfilePicture { get; set; }

        [StringLength(255)]
        public string Address { get; set; }
        
        public string Role { get; set; } = "Member"; // Default to Member
    }
}