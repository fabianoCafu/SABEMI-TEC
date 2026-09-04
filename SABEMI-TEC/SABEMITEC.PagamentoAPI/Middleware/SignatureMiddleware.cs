using System.Security.Cryptography;
using System.Text;

namespace SABEMITEC.PagamentoAPI.Middleware
{
    public class SignatureMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public SignatureMiddleware(
            RequestDelegate next,
            IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("X-Signature", out var signature))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Signature não informada.", context.RequestAborted);

                return;
            }

            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            context.Request.Body.Position = 0;

            var secret = _configuration.GetValue<string>("WebhookSecurity:SecretKey");
            var signatureCalculada = Gerar(body, secret!);

            if (!string.Equals(signature, signatureCalculada, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Signature inválida.", context.RequestAborted);

                return;
            }

            await _next(context);
        }

        private static string Gerar(string body, string secret)
        {
            var key = Encoding.UTF8.GetBytes(secret);
            var bytes = Encoding.UTF8.GetBytes(body);
            using var hmac = new HMACSHA256(key);

            return Convert.ToHexString(hmac.ComputeHash(bytes));
        }
    }
}
