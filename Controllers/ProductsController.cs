using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSWIntegral.Data;
using DSWIntegral.Models;
using Microsoft.AspNetCore.Authorization;


namespace DSWIntegral.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]                // <-- Protege todo el controlador
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            //if (true) throw new Exception("¡Prueba de excepción! TEST 2");
            
            return await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();
        }
          
 
                


        // GET: api/Products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return product;
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            // Validación SKU único
            if (await _context.Products.AnyAsync(p => p.SKU == product.SKU))
            {
                return BadRequest(new { Message = $"El SKU '{product.SKU}' ya está en uso." });
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: api/Products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, Product updatedProduct)
        {
            if (id != updatedProduct.Id)
                return BadRequest("El ID de la URL no coincide con el ID del producto.");

            // Validación SKU único (excluye el mismo registro)
            if (await _context.Products.AnyAsync(p => p.SKU == updatedProduct.SKU && p.Id != id))
            {
                return BadRequest(new { Message = $"El SKU '{updatedProduct.SKU}' ya está en uso por otro producto." });
            }

            _context.Entry(updatedProduct).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (! _context.Products.Any(p => p.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // PATCH: api/Products/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsActive = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}