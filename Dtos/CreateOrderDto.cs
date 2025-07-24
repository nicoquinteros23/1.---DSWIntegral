using System.ComponentModel.DataAnnotations;
using DSWIntegral.Dtos;

public class CreateOrderDto
{
    [Required] public Guid CustomerId { get; set; }
    [Required] public string ShippingAddress { get; set; } = null!;
    [Required] public string BillingAddress  { get; set; } = null!;
    [Required] public List<CreateOrderItemDto> Items { get; set; } = new();
}