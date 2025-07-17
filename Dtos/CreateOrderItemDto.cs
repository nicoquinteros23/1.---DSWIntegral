namespace DSWIntegral.Dtos
{
    public class CreateOrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity    { get; set; }
    }
}