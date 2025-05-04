using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bookbox.Entities
{
    public class Users
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } // First name
        
        [Required]
        [StringLength(50)]
        public string Surname { get; set; } // Last name
        
        [Required]
        [StringLength(30)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(20)]
        public string ContactNumber { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Password { get; set; }
        
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        [Required]
        public bool IsAdmin { get; set; } = false; // Changed from Role string to boolean IsAdmin
        
        [StringLength(255)]
        public string ProfilePicturePath { get; set; }
        
        // Additional attributes for user identification
        [StringLength(100)]
        public string MembershipId { get; set; }
        
        public DateTime RegistrationDate { get; set; } = DateTime.Now; // Changed from  Now
        
        // For discount tracking
        public int SuccessfulOrders { get; set; } = 0;
        
        public bool HasDiscount { get; set; } = false;
        
        [StringLength(255)]
        public string Address { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true; // Default to active when user is created
    }
}
