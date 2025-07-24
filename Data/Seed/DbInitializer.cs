// DSWIntegral/Data/Seed/DbInitializer.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DSWIntegral.Data;
using DSWIntegral.Dtos;
using DSWIntegral.Models;

namespace DSWIntegral.Data.Seed
{
    public static class DbInitializer
    {
        /// <summary>
        /// Carga datos de clientes desde un JSON si la tabla está vacía.
        /// Evita duplicar por email si ya existieran registros.
        /// </summary>
        /// <param name="context">DbContext de la aplicación.</param>
        /// <param name="jsonPath">Ruta al archivo JSON con los datos.</param>
        public static async Task SeedCustomersAsync(AppDbContext context, string jsonPath)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrWhiteSpace(jsonPath) || !File.Exists(jsonPath)) 
                throw new FileNotFoundException($"No se encontró el archivo de seed en '{jsonPath}'.");

            // Si ya hay clientes, no hacemos nada
            if (await context.Customers.AnyAsync()) 
                return;

            // Leer y deserializar
            var json = await File.ReadAllTextAsync(jsonPath);
            var customerDtos = JsonSerializer.Deserialize<List<CustomerSeedDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (customerDtos == null || customerDtos.Count == 0) 
                return;

            var hasher = new PasswordHasher<Customer>();

            foreach (var dto in customerDtos)
            {
                // Evita duplicados por email
                if (await context.Customers.AnyAsync(c => c.Email == dto.Email))
                    continue;

                var customer = new Customer
                {
                    Id        = dto.Id,
                    Email     = dto.Email,
                    Name      = dto.Name,
                    Address   = dto.Address,
                    CreatedAt = DateTime.UtcNow
                    // Role queda en su valor por defecto "Customer"
                };

                // Hasheamos la contraseña
                customer.PasswordHash = hasher.HashPassword(customer, dto.Password);

                context.Customers.Add(customer);
            }

            await context.SaveChangesAsync();
        }
    }
}