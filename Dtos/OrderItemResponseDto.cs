namespace DSWIntegral.Dtos
{
    public class OrderItemResponseDto
    {
        public Guid ProductId    { get; set; }
        public string ProductName{ get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Quantity      { get; set; }
        public decimal Subtotal  { get; set; }  // UnitPrice * Quantity
    }
}