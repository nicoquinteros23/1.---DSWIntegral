using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSWIntegral.Data;
using DSWIntegral.Models;

namespace DSWIntegral.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context)
            => _context = context;

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            // Incluimos los items (y opcionalmente el cliente)
            return await _context.Orders
                .Include(o => o.Items)
                .ToListAsync();
        }

        // GET: api/Orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) 
                return NotFound();

            return order;
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            // 1) Verificar cliente existe
            if (! await _context.Customers.AnyAsync(c => c.Id == order.CustomerId))
                return BadRequest(new { Message = $"Customer {order.CustomerId} no encontrado." });

            // 2) Verificar cada producto y stock
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return BadRequest(new { Message = $"Producto {item.ProductId} no existe." });
                if (product.StockQuantity < item.Quantity)
                    return BadRequest(new { Message = $"Stock insuficiente para SKU {product.SKU}." });

                // 3) Asignar precio actual al item y descontar stock
                item.UnitPrice = product.CurrentUnitPrice;
                product.StockQuantity -= item.Quantity;
            }

            // 4) Calcular TotalAmount
            order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // DELETE: api/Orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) 
                return NotFound();

            // (Opcional) devolver stock si cancelas:
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.StockQuantity += item.Quantity;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
