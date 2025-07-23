using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSWIntegral.Data;
using DSWIntegral.Dtos;
using DSWIntegral.Models;
using Microsoft.EntityFrameworkCore;

namespace DSWIntegral.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _ctx;
        public OrderService(AppDbContext ctx) => _ctx = ctx;

        public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto)
        {
            // 1) Validar cliente existencia
            var customer = await _ctx.Customers.FindAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException($"Customer {dto.CustomerId} no existe.");

            // 2) Instanciar orden con estado inicial
            var order = new Order
            {
                CustomerId  = dto.CustomerId,
                OrderDate   = DateTime.UtcNow,
                Status      = "Pending",
                Items       = new List<OrderItem>()
            };

            // 3) Procesar cada item: validar producto, stock y asignar precio
            foreach (var itemDto in dto.Items)
            {
                var product = await _ctx.Products.FindAsync(itemDto.ProductId)
                    ?? throw new KeyNotFoundException($"Product {itemDto.ProductId} no existe.");
                if (product.StockQuantity < itemDto.Quantity)
                    throw new InvalidOperationException($"Stock insuficiente para SKU {product.SKU}.");

                var item = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity  = itemDto.Quantity,
                    UnitPrice = product.CurrentUnitPrice
                };

                product.StockQuantity -= itemDto.Quantity;
                order.Items.Add(item);
            }

            // 4) Calcular total de la orden
            order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

            // 5) Persistir cambios
            _ctx.Orders.Add(order);
            await _ctx.SaveChangesAsync();

            // 6) Devolver DTO de respuesta
            return MapToDto(order);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetAllAsync()
        {
            var orders = await _ctx.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .ToListAsync();
            return orders.Select(MapToDto);
        }

        public async Task<OrderResponseDto?> GetByIdAsync(Guid id)
        {
            var order = await _ctx.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            return order is null ? null : MapToDto(order);
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await _ctx.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                throw new KeyNotFoundException($"Order {id} no existe.");

            // (Opcional) Restaurar stock si corresponde
            foreach (var item in order.Items)
            {
                var product = await _ctx.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.StockQuantity += item.Quantity;
            }

            _ctx.Orders.Remove(order);
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(Guid orderId, string newStatus)
        {
            var order = await _ctx.Orders.FindAsync(orderId)
                ?? throw new KeyNotFoundException($"Order {orderId} no encontrada.");

            // Validar estado válido
            var allowed = new[] { "Pending", "Processing", "Completed", "Cancelled" };
            if (!allowed.Contains(newStatus))
                throw new InvalidOperationException($"Estado '{newStatus}' no válido.");

            // Evitar retrocesos/imposibles
            if (order.Status == "Completed" || order.Status == "Cancelled")
                throw new InvalidOperationException($"No se puede cambiar el estado desde '{order.Status}'.");

            order.Status = newStatus;
            await _ctx.SaveChangesAsync();
        }

        private static OrderResponseDto MapToDto(Order order) => new()
        {
            Id          = order.Id,
            CustomerId  = order.CustomerId,
            OrderDate   = order.OrderDate,
            Status      = order.Status,
            TotalAmount = order.TotalAmount,
            Items       = order.Items.Select(i => new OrderItemResponseDto
            {
                ProductId   = i.ProductId,
                ProductName = i.Product!.Name,
                UnitPrice   = i.UnitPrice,
                Quantity    = i.Quantity,
                Subtotal    = i.UnitPrice * i.Quantity
            }).ToList()
        };
    }
}
