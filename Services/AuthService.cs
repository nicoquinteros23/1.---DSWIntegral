using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DSWIntegral.Data;
using DSWIntegral.Dtos;
using DSWIntegral.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;

namespace DSWIntegral.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _ctx;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext ctx, IConfiguration config)
        {
            _ctx    = ctx;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _ctx.Users.AnyAsync(u => u.Username == dto.Username))
                throw new InvalidOperationException("El usuario ya existe.");

            // Crear hash/salt
            using var hmac = new HMACSHA512();
            var user = new User
            {
                Username      = dto.Username,
                PasswordHash  = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)),
                PasswordSalt  = hmac.Key
            };

            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();

            // Generar token
            return CreateToken(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _ctx.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return null;

            // Verificar password
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
            if (!computed.SequenceEqual(user.PasswordHash))
                return null;

            return CreateToken(user);
        }

        private AuthResponseDto CreateToken(User user)
        {
            var jwt   = _config.GetSection("Jwt");
            var key   = Encoding.UTF8.GetBytes(jwt["Key"]!);
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresInMinutes"]!));

            var token = new JwtSecurityToken(
                issuer:  jwt["Issuer"],
                audience: jwt["Audience"],
                claims:   new List<System.Security.Claims.Claim>
                {
                    new("username", user.Username),
                    new("userid",   user.Id.ToString())
                    // opcional: new(ClaimTypes.Role, user.Role)
                },
                expires: expires,
                signingCredentials: creds
            );

            return new AuthResponseDto
            {
                Token   = new JwtSecurityTokenHandler().WriteToken(token),
                Expires = expires
            };
        }
    }
}
