using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SABEMITEC.ContratoAPI.SignalR;
using SABEMITEC.PagamentoAPI.Context;
using SABEMITEC.PagamentoAPI.Model;
using SABEMITEC.PagamentoAPI.Repository;

namespace SABEMITEC.PagamentoAPI.Test.Repository
{
    public class PagamentoRepositoryTest
    {

        private readonly Mock<IHubContext<PagamentoHub>> _hubContextMock;
        private readonly Mock<IHubClients> _hubClientsMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly Mock<ILogger<EventoBrutoRepository>> _loggerMock;

        public PagamentoRepositoryTest()
        {
            _hubContextMock = new Mock<IHubContext<PagamentoHub>>();
            _hubClientsMock = new Mock<IHubClients>();
            _clientProxyMock = new Mock<IClientProxy>();
            _loggerMock = new Mock<ILogger<EventoBrutoRepository>>();

            _hubContextMock.Setup(x => x.Clients)
                           .Returns(_hubClientsMock.Object);

            _hubClientsMock.Setup(x => x.All)
                           .Returns(_clientProxyMock.Object);
        }

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
            var mensagem = "Erro interno ao criar um EventoBruto.";
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
        public async Task CreateAsync_Deve_RetornarIsFailure_QuandoOcorrerUmaExceptionAoCriarUmEventoBrutoNoBancoDeDados()
        {
            // Arrange
            var mensagem = "Erro interno ao criar um EventoBruto.";
            var options = new DbContextOptionsBuilder<SqlSeverContextPagamento>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var contextMock = new Mock<SqlSeverContextPagamento>(options);

            contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new Exception(mensagem));

            var repository = new EventoBrutoRepository(contextMock.Object, _loggerMock.Object);

            // Act
            var result = await repository.CreateAsync(new EventoBruto());

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(mensagem, result.Error);

            _loggerMock.Verify(x => x.Log(LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao cadastra EventoBruto no banco.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);

            _clientProxyMock.Verify(x => x.SendCoreAsync("PagamentoAtualizado", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Never);
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
            var mensagem = "Erro interno ao validar se existe EventoBruto.";
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao validar se existe EventoBruto.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_Deve_RetornarIsFailure_QuandoOcorrerUmaExceptionAoValidarSeEventoBrutoExisteNaBaseDeDados()
        {
            // Arrange
            var mensagem = "Erro interno ao validar se existe EventoBruto.";
            var options = new DbContextOptionsBuilder<SqlSeverContextPagamento>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var contextMock = new Mock<SqlSeverContextPagamento>(options);

            contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new Exception(mensagem));

            var repository = new EventoBrutoRepository(contextMock.Object, _loggerMock.Object);

            // Act
            var result = await repository.ExistsEventAsync("02351414");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(mensagem, result.Error);

            _loggerMock.Verify(x => x.Log(LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao validar se existe EventoBruto.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);

            _clientProxyMock.Verify(x => x.SendCoreAsync("PagamentoAtualizado", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

