using Microsoft.AspNetCore.Http;
using SABEMITEC.PagamentoAPI.Exceptions;

namespace SABEMITEC.PagamentoAPI.Middleware
{
    public class ExceptionMiddleware
    {

        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                var jsonResponse = System.Text.Json.JsonSerializer.Serialize(new { error = ex.Message });
                await context.Response.WriteAsync(jsonResponse, context.RequestAborted);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                var jsonResponse = System.Text.Json.JsonSerializer.Serialize(new { error = ex.Message });
                await context.Response.WriteAsync(jsonResponse, context.RequestAborted);
            }
        }
    }
}
