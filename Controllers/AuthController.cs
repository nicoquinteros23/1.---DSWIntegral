using Microsoft.AspNetCore.Mvc;
using DSWIntegral.Dtos;
using DSWIntegral.Services;
using Microsoft.AspNetCore.Authorization;

namespace DSWIntegral.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                var result = await _auth.RegisterAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Ya existe el email
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var result = await _auth.LoginAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Credenciales inválidas
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}