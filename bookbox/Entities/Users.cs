using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using bookbox.Constants;

namespace bookbox.Entities
{
    public class Users
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // Initialize with a new GUID for new users
        
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
        
        public DateTime RegistrationDate { get; set; } = DateTime.Now; // Default to current date
        
        // For discount tracking
        public int SuccessfulOrders { get; set; } = 0;
        
        public bool HasDiscount { get; set; } = false;
        
        // Remove direct address storage and replace with collection of addresses
        public virtual ICollection<Address> Addresses { get; set; }
        
        public bool IsActive { get; set; } 
        
        // Add gender field
        public GenderType Gender { get; set; } // Using the enum directly
    }
}
