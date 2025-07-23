using System.Threading.Tasks;
using DSWIntegral.Dtos;

namespace DSWIntegral.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Registra un nuevo usuario y devuelve el token JWT.
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Logea un usuario existente y devuelve el token JWT, o null si falla la autenticación.
        /// </summary>
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    }
}