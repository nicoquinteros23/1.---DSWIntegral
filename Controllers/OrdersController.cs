using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using DSWIntegral.Services;
using DSWIntegral.Dtos;
using DSWIntegral.Models;

namespace DSWIntegral.Controllers
{
    /// <summary>
    /// Gestiona las órdenes de los clientes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Necesita token para todas las acciones, luego afinamos por método
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrdersController(IOrderService orderService) 
            => _orderService = orderService;

        /// <summary>
        /// GET: api/Orders/mine
        /// Lista todas las órdenes del cliente autenticado.
        /// </summary>
        [HttpGet("mine")]
        [Authorize(Policy = "CustomerOnly")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetMyOrders()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var orders = await _orderService.GetByCustomerAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Ocurrió un error interno.", Details = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/Orders
        /// Lista todas las órdenes (solo admins), opcionalmente filtradas por estado.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAllOrders([FromQuery] string? status)
        {
            try
            {
                var list = status is null
                    ? await _orderService.GetAllAsync()
                    : await _orderService.GetByStatusAsync(status);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Ocurrió un error interno.", Details = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/Orders/{id}
        /// Obtiene una orden por su identificador. Solo propietario o admin.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderResponseDto>> GetOrder(Guid id)
        {
            try
            {
                var dto = await _orderService.GetByIdAsync(id);
                if (dto == null) return NotFound();

                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (dto.CustomerId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Ocurrió un error interno.", Details = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/Orders
        /// Crea una nueva orden para el cliente autenticado.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                dto.CustomerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var created = await _orderService.CreateOrderAsync(dto);
                return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException knf)
            {
                return BadRequest(new ErrorResponse { Message = knf.Message });
            }
            catch (InvalidOperationException inv)
            {
                return BadRequest(new ErrorResponse { Message = inv.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Ocurrió un error interno.", Details = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/Orders/{id}
        /// Elimina una orden existente. Solo administradores.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            try
            {
                await _orderService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Ocurrió un error interno.", Details = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/Orders/{id}/status
        /// Actualiza el estado de una orden. Solo administradores.
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                await _orderService.UpdateStatusAsync(id, dto.NewStatus);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException inv)
            {
                return BadRequest(new ErrorResponse { Message = inv.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "Ocurrió un error interno.", Details = ex.Message });
            }
        }
    }
}