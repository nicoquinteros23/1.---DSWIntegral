using DSWIntegral.Dtos;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }

    // ← NUEVO
    public string ShippingAddress { get; set; } = null!;
    public string BillingAddress  { get; set; } = null!;
    
    public string Status { get; set; } = "Pending";

    public List<OrderItemResponseDto> Items { get; set; } = new();
}