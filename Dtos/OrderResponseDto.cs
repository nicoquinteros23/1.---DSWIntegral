namespace DSWIntegral.Dtos
{
    public class OrderResponseDto
    {
        public Guid Id            { get; set; }
        public Guid CustomerId    { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount{ get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();
        public string Status { get; set; }
    }
}