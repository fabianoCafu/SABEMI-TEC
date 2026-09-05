using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SABEMITEC.PagamentoAPI.Context;
using SABEMITEC.PagamentoAPI.Model;
using SABEMITEC.PagamentoAPI.Repository;
using Moq;

namespace PagamentoAPITest.Repository
{
    public class PagamentoRepositoryTest
    {
        private static SqlSeverContextPagamento GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SqlSeverContextPagamento>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            return new SqlSeverContextPagamento(options);
        }

        [Fact]
        public async Task CreateAsync_Deve_RetornarIsSucces_QuandoOhEventoBrutoForCadastradoComSucesso()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mockLogger = new Mock<ILogger<EventoBrutoRepository>>();
            var repository = new EventoBrutoRepository(context, mockLogger.Object);
            var payload = "{\"id_transacao\":\"000005\",\"id_contrato\":\"3265847\",\"valor\":130.55,\"data_pagamento\":\"2025-08-08T00:00:00\",\"status\":\"PARCELADO\"}";

            var eventoBruto = new EventoBruto
            {
                Id = Guid.NewGuid(),
                Payload = payload,
                DataRecebimento = DateTime.Now
            };

            // Act
            var result = await repository.CreateAsync(eventoBruto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Object);
            Assert.Equal(payload, result.Object.Payload);
        }

        [Fact]
        public async Task CreateAsync_Deve_RetornarIsFailure_QuandoOcorrerUmaExceptionAoCadastrarUmEventoBruto()
        {
            // Arrange 
            var mensagem = "Erro interno ao receber o evento.";
            var context = GetInMemoryDbContext();
            await context.DisposeAsync();
            var mockLogger = new Mock<ILogger<EventoBrutoRepository>>();
            var repository = new EventoBrutoRepository(context, mockLogger.Object);
            var evento = new EventoBruto { Id = Guid.NewGuid() };

            // Act
            var result = await repository.CreateAsync(evento);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(mensagem, result.Error);

            mockLogger.Verify(x => x.Log(LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task ExistsEventAsync_Deve_Retornar_IsFailure_QuandoOhEventoBrutoNaoExistirNoBancoDeDdados()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mockLogger = new Mock<ILogger<EventoBrutoRepository>>();
            var repository = new EventoBrutoRepository(context, mockLogger.Object);

            // Act
            var result = await repository.ExistsEventAsync("6259847");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Empty(result.Error!);
            Assert.False(result.Object); 
        }

        [Fact]
        public async Task ExistsEventAsync_Deve_RetornarFailure_QuandoOcorrerUmaExceptionAoValidarSeExisteEvento()
        {
            // Arrange
            var mensagem = "Erro interno ao validar se existe evento.";
            var context = GetInMemoryDbContext();
            var mockLogger = new Mock<ILogger<EventoBrutoRepository>>();
            var repository = new EventoBrutoRepository(context, mockLogger.Object);
            await context.DisposeAsync();

            // Act
            var result = await repository.ExistsEventAsync("9658473");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(mensagem, result.Error);

            mockLogger.Verify(x => x.Log(LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao validar se existe evento.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        //[Fact]
        //public async Task ExistsEventAsync_Deve_RetornarIsSuccess_QuandoOhEventoBrutoExistirNoBancoNoBancoDeDados()
        //{
        //    // Arrange
        //    var mockLogger = new Mock<ILogger<EventoBrutoRepository>>();
        //    var mockContext = new Mock<SqlSeverContextPagamento>();
        //    var dadosFicticios = new List<EventoBruto>()
        //    {
        //        new EventoBruto
        //        {
        //            Id = Guid.NewGuid(),
        //            Payload = "{\"id_transacao\":\"000005\",\"id_contrato\":\"3265847\",\"valor\":130.55,\"data_pagamento\":\"2025-08-08T00:00:00\",\"status\":\"PARCELADO\"}",
        //            DataRecebimento = DateTime.Now
        //        }
        //    };

        //    mockContext.Setup(c => c.LogEventosBruto).ReturnsDbSet(dadosFicticios);
        //    var repository = new EventoBrutoRepository(mockContext.Object, mockLogger.Object);

        //    // Act
        //    var result = await repository.ExistsEventAsync("000005");

        //    // Assert
        //    Assert.True(result.IsSuccess);
        //    Assert.False(result.IsFailure);
        //    Assert.True(result.Object);
        //}
    }
}
