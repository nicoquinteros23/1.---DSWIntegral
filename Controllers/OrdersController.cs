using System;
using System.Collections.Generic;
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
    [Authorize(Policy = "CustomerOnly")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrdersController(IOrderService orderService) 
            => _orderService = orderService;

        /// <summary>
        /// Lista todas las órdenes del cliente autenticado.
        /// </summary>
        /// <returns>Colección de <see cref="OrderResponseDto"/>.</returns>
        /// <response code="200">OK. Devuelve la lista de órdenes.</response>
        /// <response code="401">Unauthorized. No autenticado.</response>
        /// <response code="403">Forbidden. Sin permisos.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetOrders()
        {
            try
            {
                var orders = await _orderService.GetAllAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse {
                        Message = "Ocurrió un error interno.",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Obtiene una orden por su identificador.
        /// </summary>
        /// <param name="id">GUID de la orden.</param>
        /// <returns><see cref="OrderResponseDto"/> solicitado.</returns>
        /// <response code="200">OK. Devuelve la orden.</response>
        /// <response code="404">Not Found. Orden no existe.</response>
        /// <response code="401">Unauthorized. No autenticado.</response>
        /// <response code="403">Forbidden. Sin permisos.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
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
                var order = await _orderService.GetByIdAsync(id);
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse {
                        Message = "Ocurrió un error interno.",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Crea una nueva orden.
        /// </summary>
        /// <param name="dto"><see cref="CreateOrderDto"/> con datos de la orden.</param>
        /// <returns>Orden creada.</returns>
        /// <response code="201">Created. Orden creada correctamente.</response>
        /// <response code="400">Bad Request. Datos inválidos o recurso inexistente.</response>
        /// <response code="401">Unauthorized. No autenticado.</response>
        /// <response code="403">Forbidden. Sin permisos.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
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
                    new ErrorResponse {
                        Message = "Ocurrió un error interno.",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Elimina una orden existente, devolviendo stock si corresponde.
        /// </summary>
        /// <param name="id">GUID de la orden a eliminar.</param>
        /// <response code="204">No Content. Eliminación exitosa.</response>
        /// <response code="404">Not Found. Orden no existe.</response>
        /// <response code="401">Unauthorized. No autenticado.</response>
        /// <response code="403">Forbidden. Sin permisos.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
        [HttpDelete("{id}")]
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
                    new ErrorResponse {
                        Message = "Ocurrió un error interno.",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Actualiza el estado de una orden.
        /// </summary>
        /// <param name="id">GUID de la orden.</param>
        /// <param name="dto"><see cref="UpdateOrderStatusDto"/> con el nuevo estado.</param>
        /// <response code="204">No Content. Estado actualizado.</response>
        /// <response code="400">Bad Request. Estado inválido.</response>
        /// <response code="404">Not Found. Orden no existe.</response>
        /// <response code="401">Unauthorized. No autenticado.</response>
        /// <response code="403">Forbidden. Sin permisos.</response>
        /// <response code="500">Internal Server Error. Error inesperado.</response>
        [HttpPut("{id}/status")]
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
                    new ErrorResponse {
                        Message = "Ocurrió un error interno.",
                        Details = ex.Message
                    });
            }
        }
    }
}