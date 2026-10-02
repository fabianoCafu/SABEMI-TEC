using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SABEMITEC.PagamentoAPI.Exceptions;
using System.Text.Json;

namespace SABEMITEC.PagamentoAPI.Middleware.Tests
{
    public class ExceptionMiddlewareTests
    {
        private readonly DefaultHttpContext _context;
        private readonly MemoryStream _responseBodyStream;

        public ExceptionMiddlewareTests()
        {
            _context = new DefaultHttpContext();
            _responseBodyStream = new MemoryStream();
            _context.Response.Body = _responseBodyStream;
        }

        [Fact]
        public async Task InvokeAsync_QuandoLancarNotFoundException_DeveRetornarStatus404EMensagemCorreta()
        {
            // Arrange
            var mensagem = "O recurso solicitado não foi localizado.";
            RequestDelegate next = (ctx) => throw new NotFoundException(mensagem);
            var middleware = new ExceptionMiddleware(next);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert 
            _context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            _context.Response.ContentType.Should().Be("application/json");
            var jsonResponse = await ReadResponseBodyAsync();
            var resultado = JsonSerializer.Deserialize<ErrorResponse>(jsonResponse);

            resultado.Should().NotBeNull();
            resultado.Error.Should().Be(mensagem);
        }

        [Fact]
        public async Task InvokeAsync_QuandoLancarExceptionGenerica_DeveRetornarStatus500EMensagemCorreta()
        {
            // Arrange
            var mensagem = "Erro genérico no servidor.";
            RequestDelegate next = (ctx) => throw new Exception(mensagem);
            var middleware = new ExceptionMiddleware(next);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            _context.Response.ContentType.Should().Be("application/json");
            var jsonResponse = await ReadResponseBodyAsync();
            var resultado = JsonSerializer.Deserialize<ErrorResponse>(jsonResponse);
            resultado.Should().NotBeNull();
            resultado.Error.Should().Be(mensagem);
        }

        [Fact]
        public async Task InvokeAsync_QuandoFluxoForSucesso_DeveManterStatus200()
        {
            // Arrange
            RequestDelegate next = (ctx) =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                return Task.CompletedTask;
            };
            var middleware = new ExceptionMiddleware(next);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        #region Metodos Private

        private async Task<string> ReadResponseBodyAsync()
        {
            _responseBodyStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_responseBodyStream);
            return await reader.ReadToEndAsync();
        }

        private class ErrorResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("error")]
            public string ?Error { get; set; }
        }

        #endregion
    }
}
