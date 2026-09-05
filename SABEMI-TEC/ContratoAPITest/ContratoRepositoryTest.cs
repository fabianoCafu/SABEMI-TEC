using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SABEMITEC.ContratoAPI.Context;
using SABEMITEC.ContratoAPI.Models;
using SABEMITEC.ContratoAPI.Repository;
using SABEMITEC.ContratoAPI.SignalR;
using Moq;

namespace SABEMITEC.ContratoAPI.Test.Repository
{
    public class ContratoRepositoryTest
    {
        private readonly Mock<IHubContext<PagamentoHub>> _hubContextMock;
        private readonly Mock<IHubClients> _hubClientsMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly Mock<ILogger<ContratoRepository>> _loggerMock;

        public ContratoRepositoryTest()
        {
            _hubContextMock = new Mock<IHubContext<PagamentoHub>>();
            _hubClientsMock = new Mock<IHubClients>();
            _clientProxyMock = new Mock<IClientProxy>();
            _loggerMock = new Mock<ILogger<ContratoRepository>>();

            _hubContextMock.Setup(x => x.Clients)
                           .Returns(_hubClientsMock.Object);

            _hubClientsMock.Setup(x => x.All)
                           .Returns(_clientProxyMock.Object);
        }

        private static SqlSeverContextContrato CreateContext()
        {
            var options = new DbContextOptionsBuilder<SqlSeverContextContrato>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new SqlSeverContextContrato(options);
        }

        private ContratoRepository CreateRepository(SqlSeverContextContrato context)
        {
            return new ContratoRepository(_hubContextMock.Object, context, _loggerMock.Object);
        }

        [Fact]
        public async Task GetListContractAsync_Deve_RetornarIsSucces_QuandoListarStatusContratoOrdenadaPorData()
        {
            // Arrange
            await using var context = CreateContext();
            var listaStatusContrato = ObterlistaStatusContrato();  
            context.StatusContrato!.AddRange(listaStatusContrato);
            await context.SaveChangesAsync();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetListContractAsync();
            var listaOrdenadoPorData = result.Object!.OrderBy(a => a.DataProcessamento).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Object);
            Assert.Equal(listaStatusContrato.Count, listaOrdenadoPorData.Count);
            Assert.Equal(listaStatusContrato[0].IdContrato, listaOrdenadoPorData[0].IdContrato);
            Assert.Equal(listaStatusContrato[1].IdContrato, listaOrdenadoPorData[1].IdContrato);
            Assert.Equal(listaStatusContrato[2].IdContrato, listaOrdenadoPorData[2].IdContrato);
            Assert.Equal(listaStatusContrato[3].IdContrato, listaOrdenadoPorData[3].IdContrato);   
        }

        [Fact]
        public async Task GetListContractAsync_Deve_RetornarIsSucces_QuandoRetornarUmaListaVazia()
        {
            // Arrange
            await using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetListContractAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Object);
            Assert.Empty(result.Object);
        }

        [Fact]
        public async Task CreateAsync_Deve_RetornarIsSucces_QuandoOhStatusContratoForSalvoComSucesso()
        {
            // Arrange
            await using var context = CreateContext();

            var repository = CreateRepository(context);
            var statusContrato = ObterlistaStatusContrato()[0];
            
            _clientProxyMock.Setup(x => x.SendCoreAsync("PagamentoAtualizado", It.IsAny<object?[]>(),It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await repository.CreateAsync(statusContrato);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Object); 
            Assert.Equal(statusContrato.IdContrato, result.Object.IdContrato);
            var statusContratoPersistido = await context.StatusContrato!.FirstOrDefaultAsync(x => x.IdContrato == "368547");
            Assert.NotNull(statusContratoPersistido);
            Assert.Equal("000001", statusContratoPersistido.IdTransacao);
        }

        [Fact]
        public async Task CreateAsync_Deve_RetornarUmaNotificaoViaSignalR()
        {
            // Arrange
            await using var context = CreateContext();
            var repository = CreateRepository(context);
            var statusContrato = ObterlistaStatusContrato()[1];
            
            _clientProxyMock.Setup(x => x.SendCoreAsync("PagamentoAtualizado", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await repository.CreateAsync(statusContrato);

            // Assert
            Assert.True(result.IsSuccess);
            _clientProxyMock.Verify(x => x.SendCoreAsync("PagamentoAtualizado", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()),Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_Deve_RetornarIsSucces_QuandoOhContratoJaExistirNaBaseDeDados()
        {
            // Arrange
            await using var context = CreateContext();
            context.StatusContrato!.Add(ObterlistaStatusContrato()[2]); 
            await context.SaveChangesAsync();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.ExistsAsync("363629", "000003");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task ExistsAsync_Deve_RetornarIsFailure_QuandoOhContratoNaoExistirNaBaseDeDados()
        {
            // Arrange
            await using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.ExistsAsync("361247", "000004");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Message);
        }

        [Fact]
        public async Task ExistsAsync_Deve_RetornarIsFailure_QuandoTentarSalvarAhMesmaTransacaoComContratoDiferente()
        {
            // Arrange
            await using var context = CreateContext();
            context.StatusContrato!.Add(ObterlistaStatusContrato()[2]); 
            await context.SaveChangesAsync();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.ExistsAsync("368547", "000003");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Message);
            Assert.True(result.IsFailure);
        }

        #region Metodos Private

        private static List<StatusContrato> ObterlistaStatusContrato()
        {
            return new List<StatusContrato>
            {
                new StatusContrato("000001", "368547", "SUCESSO", ""),
                new StatusContrato("000002", "368325", "ERRO", "O atributo 'valor' deve ser maior que 0!"),
                new StatusContrato("000003", "363629", "SUCESSO", ""),
                new StatusContrato("000004", "361247", "ERRO", "O atributo 'status' é obrigatório!")
            };
        }

        #endregion
    }
}

