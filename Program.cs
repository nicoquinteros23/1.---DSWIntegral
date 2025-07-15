using System.Linq;
using System.Text.Json;
using DSWIntegral.Data;
using DSWIntegral.Middleware;
using DSWIntegral.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1) Servicios

// 1.1 DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1.2 Controllers + validación unificada de ModelState
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(err => err.ErrorMessage).ToArray()
                );

            var errorResponse = new ErrorResponse
            {
                Message = "Uno o más errores de validación ocurrieron.",
                Details = JsonSerializer.Serialize(errors)
            };

            return new BadRequestObjectResult(errorResponse);
        };
    });

// 1.3 Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1.4 CORS: permitir frontend en localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// 2) Middleware global de excepciones
app.UseMiddleware<GlobalExceptionMiddleware>();

// 3) Swagger UI (solo en Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DSWIntegral API V1");
        c.RoutePrefix = string.Empty; // monta UI en http://localhost:5000/
    });
}

// 4) Pipeline de HTTP

app.UseHttpsRedirection();

// 4.1 CORS
app.UseCors("AllowLocalhost3000");

// 4.2 Autorizar (token, políticas, etc.)
app.UseAuthorization();

// 4.3 Mapear controllers
app.MapControllers();

app.Run();
