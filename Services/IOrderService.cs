using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSWIntegral.Dtos;

namespace DSWIntegral.Services
{
    public interface IOrderService
    {
        // Crear orden
        Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto);

        // Listar todas (admins) o por cliente
        Task<IEnumerable<OrderResponseDto>> GetAllAsync();
        Task<IEnumerable<OrderResponseDto>> GetByCustomerAsync(Guid customerId);
        
        // Listar por estado
        Task<IEnumerable<OrderResponseDto>> GetByStatusAsync(string status);
        
        

        // Obtener por id
        Task<OrderResponseDto?> GetByIdAsync(Guid id);

        // Eliminar orden
        Task DeleteAsync(Guid id);

        // Cambiar estado
        Task UpdateStatusAsync(Guid orderId, string newStatus);
    }
}