using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DSWIntegral.Models
{
    // Índice en CustomerId para optimizar consultas por cliente
    [Index(nameof(CustomerId))]
    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // FK a Customer
        [Required]
        public Guid CustomerId { get; set; }

        // Navegación a Customer
        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = default!;

        // Fecha de creación del pedido
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Total calculado o almacenado
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Items del pedido
        public List<OrderItem> Items { get; set; } = new();
    }
}