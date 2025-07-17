using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DSWIntegral.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Customer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = default!;

        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}