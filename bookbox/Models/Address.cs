using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bookbox.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Address Line 1")]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        public string Address2 { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [StringLength(50)]
        public string State { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string Zip { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }
        
        [Required]
        [Display(Name = "Primary Address")]
        public bool IsPrimary { get; set; } = false;
    }
}