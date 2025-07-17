using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DSWIntegral.Dtos;
using DSWIntegral.Models;
using DSWIntegral.Services;

namespace DSWIntegral.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/Orders
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetOrders()
            => Ok(await _orderService.GetAllAsync());

        // GET: api/Orders/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderResponseDto>> GetOrder(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            return order is null ? NotFound() : Ok(order);
        }

        // POST: api/Orders
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
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
        }

        // DELETE: api/Orders/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        }

        // PUT: api/Orders/{id}/status
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        }
    }
}
