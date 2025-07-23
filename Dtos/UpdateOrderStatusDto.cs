namespace DSWIntegral.Dtos
{
    public class UpdateOrderStatusDto
    {
        /// <summary>
        /// Nuevo estado de la orden. Por ejemplo: "Pending", "Processing", "Completed", "Cancelled".
        /// </summary>
        public string NewStatus { get; set; } = string.Empty;
    }
}