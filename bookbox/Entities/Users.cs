using System.ComponentModel.DataAnnotations;

namespace bookbox.Entities
{
    public class Users
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "Admin" or "User"
    }
}
