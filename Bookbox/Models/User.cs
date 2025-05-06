using System;
using System.ComponentModel.DataAnnotations;
using Bookbox.Constants;

namespace Bookbox.Models
{
    public class User
    {
        [Key]
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        [StringLength(20)]
        public string? ContactNo { get; set; }
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public Gender Gender { get; set; }
        
        public string? ImageUrlText { get; set; }
        
        [Required]
        public DateTimeOffset RegisteredDate { get; set; } = DateTimeOffset.Now;
        
        [Required]
        public UserType UserType { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        // Add a counter for successful orders
        public int SuccessfulOrderCount { get; set; } = 0;
        
        // Navigation properties
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    }
}