using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DSWIntegral.Models
{
    [Index(nameof(SKU), IsUnique = true)]
    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string SKU { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal CurrentUnitPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;
    }
}