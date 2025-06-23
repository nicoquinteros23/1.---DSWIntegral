using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DSWIntegral.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar DbContext con EF Core y SQL Server
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
              ?? "Server=localhost,1433;Database=DSW2025;User Id=sa;Password=TuP4ssw0rd!;TrustServerCertificate=True;";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connStr));

// 2. Añadir servicios de controllers
builder.Services.AddControllers();

// 3. Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        name: "v1",
        info: new OpenApiInfo { Title = "DSWIntegral API", Version = "v1" }
    );
});

var app = builder.Build();

// 4. Middleware de desarrollo: Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DSWIntegral API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz (opcional)
    });
}

// 5. Middlewares globales
app.UseRouting();
app.UseAuthorization();

// 6. Mapear controllers
app.MapControllers();

app.Run();