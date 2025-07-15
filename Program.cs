using DSWIntegral.Data;
using DSWIntegral.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1) Servicios
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

// 2) Middleware global de errores
app.UseMiddleware<GlobalExceptionMiddleware>();   //<-p/ probar la excepcion

// 3) Swagger solo en Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DSWIntegral API V1");
        c.RoutePrefix = string.Empty;    // opcional: monta Swagger UI en la raíz http://localhost:5000/
    });
}

// 4) Pipeline
app.UseHttpsRedirection();
app.UseCors("AllowLocalhost3000");
app.UseAuthorization();
app.MapControllers();
app.Run();