using System;
using System.ComponentModel.DataAnnotations;

namespace DSWIntegral.Models
{
    public class Customer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string PasswordHash { get; set; }    // <-- Nueva propiedad

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}