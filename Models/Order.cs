using System.ComponentModel.DataAnnotations;
using DSWIntegral.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }

    // ← NUEVO
    [Required]
    [MaxLength(200)]
    public string ShippingAddress { get; set; } = null!;
    [Required]
    [MaxLength(200)]
    public string BillingAddress  { get; set; } = null!;
    
    [Required]
    public string Status { get; set; } = "Pending";

    public List<OrderItem> Items { get; set; } = new();
}