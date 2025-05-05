using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using bookbox.Constants;

namespace bookbox.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string Name { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string Surname { get; set; }
        
        [Required]
        [StringLength(30)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }
        
        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }
        
        [Required]
        public bool IsAdmin { get; set; } = false;
        
        [StringLength(255)]
        public string ProfilePicturePath { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Membership ID")]
        public string MembershipId { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
        public int SuccessfulOrders { get; set; } = 0;
        
        public bool HasDiscount { get; set; } = false;
        
        public virtual ICollection<Address> Addresses { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public GenderType Gender { get; set; }
    }
}