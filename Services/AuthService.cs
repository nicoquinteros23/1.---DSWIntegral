using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DSWIntegral.Data;
using DSWIntegral.Dtos;
using DSWIntegral.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DSWIntegral.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _ctx;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<Customer> _hasher;

        public AuthService(AppDbContext ctx, IConfiguration config)
        {
            _ctx    = ctx;
            _config = config;
            _hasher = new PasswordHasher<Customer>();
        }

        // Services/AuthService.cs (método RegisterAsync)
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _ctx.Customers.AnyAsync(c => c.Email == dto.Email))
                throw new InvalidOperationException($"El email '{dto.Email}' ya existe.");

            var customer = new Customer
            {
                Name    = dto.Name,
                Email   = dto.Email,
                Address = dto.Address,
                Role    = dto.Role ?? "Customer"    // ← Asigna rol
            };

            customer.PasswordHash = _hasher.HashPassword(customer, dto.Password);
            _ctx.Customers.Add(customer);
            await _ctx.SaveChangesAsync();

            return GenerateToken(customer);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var customer = await _ctx.Customers
                .SingleOrDefaultAsync(c => c.Email == dto.Email)
                ?? throw new KeyNotFoundException("Credenciales inválidas.");

            var result = _hasher.VerifyHashedPassword(customer, customer.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new KeyNotFoundException("Credenciales inválidas.");

            return GenerateToken(customer);
        }

        // Services/AuthService.cs (método GenerateToken)
        private AuthResponseDto GenerateToken(Customer customer)
        {
            var jwtSection       = _config.GetSection("Jwt");
            var keyBytes         = Encoding.UTF8.GetBytes(jwtSection["Key"]!);
            var issuer           = jwtSection["Issuer"]!;
            var audience         = jwtSection["Audience"]!;
            var expiresInMinutes = jwtSection.GetValue<int>("ExpiresInMinutes");

            // Aquí añadimos la claim de Role
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, customer.Email),
                new Claim(ClaimTypes.Role, customer.Role),   // ← Role claim
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256
            );

            var expires = DateTime.UtcNow.AddMinutes(expiresInMinutes);

            var token = new JwtSecurityToken(
                issuer:             issuer,
                audience:           audience,
                claims:             claims,
                expires:            expires,
                signingCredentials: creds
            );

            return new AuthResponseDto {
                Token   = new JwtSecurityTokenHandler().WriteToken(token),
                Expires = expires
            };
        } 
    }
}
