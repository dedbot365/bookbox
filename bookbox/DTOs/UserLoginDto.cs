using System.ComponentModel.DataAnnotations;

namespace bookbox.DTOs
{
    public class UserLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
        
        public bool RememberMe { get; set; } = false;
    }
}