using DSWIntegral.Data;
using DSWIntegral.Models;
using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static async Task SeedCustomersAsync(AppDbContext ctx, string jsonPath, ILogger logger)
    {
        try
        {
            if (await ctx.Customers.AnyAsync())
            {
                logger.LogInformation("Customers already exist in the database. Skipping seeding.");
                return;
            }

            var json = await File.ReadAllTextAsync(jsonPath);
            var customers = System.Text.Json.JsonSerializer.Deserialize<List<Customer>>(json);

            if (customers == null || !customers.Any())
            {
                logger.LogWarning("No valid customers found in JSON file at '{jsonPath}'.", jsonPath);
                return;
            }

            foreach (var customer in customers)
            {
                if (string.IsNullOrWhiteSpace(customer.Name) || 
                    string.IsNullOrWhiteSpace(customer.Email))
                {
                    logger.LogWarning("Invalid customer record skipped: {Customer}", customer);
                    continue;
                }

                if (!await ctx.Customers.AnyAsync(c => c.Email == customer.Email))
                {
                    ctx.Customers.Add(customer);
                }
            }

            await ctx.SaveChangesAsync();
            logger.LogInformation("Successfully seeded {Count} customers from '{JsonPath}'.", customers.Count, jsonPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding customers.");
        }
    }
}