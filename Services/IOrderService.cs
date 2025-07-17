using DSWIntegral.Dtos;

namespace DSWIntegral.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto);
        Task<IEnumerable<OrderResponseDto>> GetAllAsync();
        Task<OrderResponseDto?> GetByIdAsync(Guid id);
        Task DeleteAsync(Guid id);
        
        /// <summary>
        /// Cambia el estado de una orden existente.
        /// </summary>
        Task UpdateStatusAsync(Guid orderId, string newStatus);

    }
}