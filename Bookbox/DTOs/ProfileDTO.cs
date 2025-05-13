using Bookbox.Constants;
using Bookbox.Models;
using System.ComponentModel.DataAnnotations;

namespace Bookbox.DTOs
{
    public class ProfileDTO
    {
        public string Username { get; set; } = string.Empty;
        
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? ContactNo { get; set; }
        
        public DateTime? DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        public string? ImageUrl { get; set; }
        
        public int SuccessfulOrderCount { get; set; }
        
        public DateTimeOffset RegisteredDate { get; set; }
        
        // Address information
        public string Address1 { get; set; } = string.Empty;
        
        public string? Address2 { get; set; }
        
        public string City { get; set; } = string.Empty;
        
        public string State { get; set; } = string.Empty;
        
        public string Country { get; set; } = string.Empty;
    }
}
