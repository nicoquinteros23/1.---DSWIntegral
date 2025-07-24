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
            // Validar cliente existencia
            var customer = await _ctx.Customers.FindAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException($"Customer {dto.CustomerId} no existe.");

            // Instanciar orden con direcciones
            var order = new Order
            {
                CustomerId      = dto.CustomerId,
                OrderDate       = DateTime.UtcNow,
                ShippingAddress = dto.ShippingAddress,
                BillingAddress  = dto.BillingAddress,
                Items           = new List<OrderItem>()
            };

            // Procesar items
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

            // Calcular total
            order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);
            _ctx.Orders.Add(order);
            await _ctx.SaveChangesAsync();

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
        
        public async Task<IEnumerable<OrderResponseDto>> GetByStatusAsync(string status)
        {
            var orders = await _ctx.Orders
                .Where(o => o.Status == status)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .ToListAsync();
            return orders.Select(MapToDto);
        }

        // Nuevo método para cumplir la interfaz
        public async Task<IEnumerable<OrderResponseDto>> GetByCustomerAsync(Guid customerId)
        {
            var orders = await _ctx.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .ToListAsync();
            return orders.Select(MapToDto);
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await _ctx.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                throw new KeyNotFoundException($"Order {id} no existe.");

            // Restaurar stock
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

            var allowed = new[] { "Pending", "Processing", "Completed", "Cancelled" };
            if (!allowed.Contains(newStatus))
                throw new InvalidOperationException($"Estado '{newStatus}' no válido. Debe ser uno de: {string.Join(", ", allowed)}.");

            if (order.Status == "Completed" || order.Status == "Cancelled")
                throw new InvalidOperationException($"No se puede cambiar el estado desde '{order.Status}'.");

            order.Status = newStatus;
            await _ctx.SaveChangesAsync();
        }

        private static OrderResponseDto MapToDto(Order order) => new()
        {
            Id               = order.Id,
            CustomerId       = order.CustomerId,
            OrderDate        = order.OrderDate,
            Status           = order.Status,
            ShippingAddress  = order.ShippingAddress,
            BillingAddress   = order.BillingAddress,
            TotalAmount      = order.TotalAmount,
            Items            = order.Items.Select(i => new OrderItemResponseDto
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