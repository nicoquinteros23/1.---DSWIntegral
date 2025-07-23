using System;
using System.ComponentModel.DataAnnotations;

namespace DSWIntegral.Models
{
    public class User
    {
        [Key]
        public Guid   Id       { get; set; } = Guid.NewGuid();
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public byte[] PasswordHash { get; set; }
        [Required]
        public byte[] PasswordSalt { get; set; }
        // Opcional: public string Role { get; set; } = "User";
    }
}