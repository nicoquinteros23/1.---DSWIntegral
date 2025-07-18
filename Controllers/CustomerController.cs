using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSWIntegral.Data;
using DSWIntegral.Dtos;
using DSWIntegral.Models;

namespace DSWIntegral.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // <-- Todos los endpoints requieren token válido
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
            => _context = context;

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            var list = await _context.Customers.ToListAsync();
            var dtos = list.Select(c => new CustomerDto {
                Id        = c.Id,
                Name      = c.Name,
                Email     = c.Email,
                Address   = c.Address,
                CreatedAt = c.CreatedAt
            });
            return Ok(dtos);
        }

        // GET: api/Customers/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id)
        {
            var c = await _context.Customers.FindAsync(id);
            if (c == null) return NotFound();
            var dto = new CustomerDto {
                Id        = c.Id,
                Name      = c.Name,
                Email     = c.Email,
                Address   = c.Address,
                CreatedAt = c.CreatedAt
            };
            return Ok(dto);
        }

        // POST: api/Customers
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createDto)
        {
            // 1) Validar email único
            if (await _context.Customers.AnyAsync(c => c.Email == createDto.Email))
            {
                return BadRequest(new { Message = $"El email '{createDto.Email}' ya existe." });
            }

            // 2) Mapear a entidad
            var customer = new Customer {
                Name    = createDto.Name,
                Email   = createDto.Email,
                Address = createDto.Address
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // 3) Devolver DTO de respuesta
            var dto = new CustomerDto {
                Id        = customer.Id,
                Name      = customer.Name,
                Email     = customer.Email,
                Address   = customer.Address,
                CreatedAt = customer.CreatedAt
            };

            return CreatedAtAction(nameof(GetCustomer),
                                   new { id = dto.Id },
                                   dto);
        }

        // PUT: api/Customers/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, UpdateCustomerDto updateDto)
        {
            if (id != updateDto.Id)
                return BadRequest("El ID de la URL no coincide con el ID del customer.");

            // Validar email único (excepto este registro)
            if (await _context.Customers
                   .AnyAsync(c => c.Email == updateDto.Email && c.Id != id))
            {
                return BadRequest(new { Message = $"El email '{updateDto.Email}' ya está en uso." });
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            // Aplicar cambios
            customer.Name    = updateDto.Name;
            customer.Email   = updateDto.Email;
            customer.Address = updateDto.Address;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (! await _context.Customers.AnyAsync(c => c.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Customers/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
