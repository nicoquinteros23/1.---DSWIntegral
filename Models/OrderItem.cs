using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DSWIntegral.Models
{
    // Índice en OrderId y ProductId para búsquedas frecuentes
    [Index(nameof(OrderId))]
    [Index(nameof(ProductId))]
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // FK a Order
        [Required]
        public Guid OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = default!;

        // FK a Product
        [Required]
        public Guid ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = default!;

        // Cantidad pedida
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        // Precio unitario en el momento del pedido
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}