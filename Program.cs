using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AspNetCoreRateLimit;
using DSWIntegral.Data;
using DSWIntegral.Dtos; 
using DSWIntegral.Models;
using DSWIntegral.Middleware;
using DSWIntegral.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DSWIntegral.Data.Seed;

// Para mostrar detalles de error de JWT en consola
IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

// 1) Servicios
// 1.1 DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1.2 Controllers + validación de ModelState
builder.Services.AddControllers()
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

// 1.3 Swagger / OpenAPI con JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DSWIntegral API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [ new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } } ] = Array.Empty<string>()
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// 1.4 CORS: permitir frontend localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// 1.5 Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// 1.6 Inyección de dependencias de servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// 1.7 JWT Authentication + Authorization Policies
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes   = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken            = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer           = false, // ajustar si quieres forzar issuer
            ValidIssuer              = jwtSection["Issuer"],
            ValidateAudience         = false, // ajustar si quieres forzar audience
            ValidAudience            = jwtSection["Audience"],
            ValidateLifetime         = true,
            RoleClaimType            = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                Console.WriteLine("RAW AUTH HEADER: " + ctx.Request.Headers["Authorization"]);
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("JWT failed: " + ctx.Exception);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole("Customer"));
});

// 2) Construir la aplicación
var app = builder.Build();

// 3) Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DSWIntegral API V1");
        c.RoutePrefix = string.Empty;
    });
}

// 4) Pipeline HTTP
app.UseRouting();
app.UseCors("AllowLocalhost3000");
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();





// tras builder.Build();
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var path = Path.Combine(app.Environment.ContentRootPath, "Data/Seed/customers.json");
    await DbInitializer.SeedCustomersAsync(ctx, path);
}

app.MapControllers();
app.Run();

public partial class Program {}

