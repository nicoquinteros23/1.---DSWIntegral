using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using DSWIntegral.Models;

namespace DSWIntegral.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public GlobalExceptionMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // 1️⃣ Log si lo deseas
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var error = new ErrorResponse
                {
                    Message = "Ocurrió un error interno.",
                    Details = context.RequestServices
                        .GetService<IWebHostEnvironment>()!
                        .IsDevelopment()
                        ? ex.Message
                        : null
                };

                var json = JsonSerializer.Serialize(error);
                await context.Response.WriteAsync(json);
            }
        }
    }
}