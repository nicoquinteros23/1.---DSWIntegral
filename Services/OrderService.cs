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
            // 1) Validar cliente
            var customer = await _ctx.Customers.FindAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException($"Customer {dto.CustomerId} no existe.");

            // 2) Crear entidad Order
            var order = new Order {
                CustomerId = dto.CustomerId,
                OrderDate  = DateTime.UtcNow,
                Items      = new List<OrderItem>()
            };

            // 3) Por cada item: validar producto, stock, asignar precio…
            foreach (var itemDto in dto.Items)
            {
                var product = await _ctx.Products.FindAsync(itemDto.ProductId)
                    ?? throw new KeyNotFoundException($"Product {itemDto.ProductId} no existe.");
                if (product.StockQuantity < itemDto.Quantity)
                    throw new InvalidOperationException($"Stock insuficiente para SKU {product.SKU}.");

                var item = new OrderItem {
                    ProductId = product.Id,
                    Quantity  = itemDto.Quantity,
                    UnitPrice = product.CurrentUnitPrice
                };

                product.StockQuantity -= itemDto.Quantity;
                order.Items.Add(item);
            }

            // 4) Calcular total
            order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

            // 5) Persistir
            _ctx.Orders.Add(order);
            await _ctx.SaveChangesAsync();

            // 6) Mapear a DTO de respuesta
            return new OrderResponseDto {
                Id          = order.Id,
                CustomerId  = order.CustomerId,
                OrderDate   = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(i => new OrderItemResponseDto {
                    ProductId   = i.ProductId,
                    ProductName = i.Product!.Name,
                    UnitPrice   = i.UnitPrice,
                    Quantity    = i.Quantity,
                    Subtotal    = i.UnitPrice * i.Quantity
                }).ToList()
            };
        }

        public async Task<IEnumerable<OrderResponseDto>> GetAllAsync()
        {
            var orders = await _ctx.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .ToListAsync();
            return orders.Select(o => new OrderResponseDto {
                Id          = o.Id,
                CustomerId  = o.CustomerId,
                OrderDate   = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Items = o.Items.Select(i => new OrderItemResponseDto {
                    ProductId   = i.ProductId,
                    ProductName = i.Product!.Name,
                    UnitPrice   = i.UnitPrice,
                    Quantity    = i.Quantity,
                    Subtotal    = i.UnitPrice * i.Quantity
                }).ToList()
            });
        }

        public async Task<OrderResponseDto?> GetByIdAsync(Guid id)
        {
            var o = await _ctx.Orders
                .Include(x => x.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (o == null) return null;
            return new OrderResponseDto {
                Id          = o.Id,
                CustomerId  = o.CustomerId,
                OrderDate   = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Items = o.Items.Select(i => new OrderItemResponseDto {
                    ProductId   = i.ProductId,
                    ProductName = i.Product!.Name,
                    UnitPrice   = i.UnitPrice,
                    Quantity    = i.Quantity,
                    Subtotal    = i.UnitPrice * i.Quantity
                }).ToList()
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await _ctx.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) throw new KeyNotFoundException($"Order {id} no existe.");
            // (Opcional) devolver stock...
            _ctx.Orders.Remove(order);
            await _ctx.SaveChangesAsync();
        }
    }
}
