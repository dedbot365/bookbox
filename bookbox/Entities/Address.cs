using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bookbox.Entities
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [StringLength(50)]
        public string State { get; set; }

        [Required]
        [StringLength(20)]
        public string Zip { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual Users User { get; set; }
        
        [Required]
        public bool IsPrimary { get; set; } = false;
    }
}