using Microsoft.Extensions.Logging;
using Moq;
using SABEMITEC.ContratoAPI.Models;
using SABEMITEC.ContratoAPI.Repository;
using SABEMITEC.ContratoAPI.Service;
using SABEMITEC.Shared.Event;
using static SABEMITEC.Shared.PartnerResult;

namespace ContratoAPITest
{
    public class ContratoServiceTest
    {
        private readonly Mock<IContratoRepository> _mockContratoRepository;
        private readonly Mock<ILogger<ContratoService>> _mockLogger;

        public ContratoServiceTest()
        {
            _mockContratoRepository = new Mock<IContratoRepository>();
            _mockLogger = new Mock<ILogger<ContratoService>>();
        }

        #region Metodo GetListContractAsync

        [Fact]
        public async void GetListContractAsync_Deve_RetornarSucesso_QuandoNaoExistirNenhumStatusContratoCadastrado()
        {
            // Arrange
            var mensagem = "Não existe nemhum pagamento Processado!";
            var statusContratos = new List<StatusContrato>();

            _mockContratoRepository.Setup(x => x.GetListContractAsync())
                                   .ReturnsAsync(Result<List<StatusContrato>>.Success(statusContratos));

            var contratoService = new ContratoService(_mockContratoRepository.Object, _mockLogger.Object);

            // Act
            var result = await contratoService.GetListContractAsync();

            // Assert 
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Null(result.Object);
            Assert.Equal(mensagem, result.Message);

            _mockContratoRepository.Verify(x => x.GetListContractAsync(), Times.Once);
        }

        [Fact]
        public async void GetListContractAsync_Deve_RetornarSucesso_QuandoExistirStatusContratoCadastrado()
        {
            // Arrange
            var statusContratos = new List<StatusContrato>
            {
                new StatusContrato("000001", "368547", "SUCESSO", ""),
                new StatusContrato("000002", "368325", "ERRO", "O atributo 'valor' deve ser maior que 0!"),
                new StatusContrato("000003", "363629", "SUCESSO", ""),
                new StatusContrato("000004", "361247", "ERRO", "O atributo 'status' é obrigatório!")
            };

            _mockContratoRepository.Setup(x => x.GetListContractAsync())
                                   .ReturnsAsync(Result<List<StatusContrato>>.Success(statusContratos));

            var contratoService = new ContratoService(_mockContratoRepository.Object, _mockLogger.Object);

            // Act
            var result = await contratoService.GetListContractAsync();

            // Assert 
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Object);
            Assert.Null(result.Message);

            _mockContratoRepository.Verify(x => x.GetListContractAsync(), Times.Once);
        }

        [Fact]
        public async void GetListContractAsync_Deve_RetornarUmaExcption_QuandoBuscarOsStatusContrato()
        {
            // Arrange 
            var mensagem = "Ocorreu um erro interno no servidor.";
            _mockContratoRepository.Setup(r => r.GetListContractAsync())
                                    .ThrowsAsync(new Exception(string.Empty));

            var contratoService = new ContratoService(_mockContratoRepository.Object, _mockLogger.Object);

            // Act
            var result = await contratoService.GetListContractAsync();

            // Assert
            var statusCodeResult = Assert.IsType<Result<List<StatusContrato>>>(result);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Null(result.Object);
            Assert.Equal(mensagem, result.Error);

            _mockContratoRepository.Verify(x => x.GetListContractAsync(), Times.Once); 
        }

        #endregion

        #region Metodo CreateContractStatusAsync

        [Fact]
        public async Task CreateContractStatusAsync_Deve_RetornarSucesso_QuandoStatusContratoForCadastradoSucesso()
        {
            // Arrange 
            var mensagem = "StatusContrato cadastrado com Sucesso!";
            var eventoStatusContrato = new EventoStatusContrato("000001", "368547", "SUCESSO","");
            
            _mockContratoRepository.Setup(x => x.CreateAsync(It.IsAny<StatusContrato>()))
                                   .ReturnsAsync(Result<StatusContrato>.Success(new StatusContrato("0012514","000001", "SUCESSO", "")));

            var contratoService = new ContratoService(_mockContratoRepository.Object, _mockLogger.Object);

            // Act
            var result = await contratoService.CreateContractStatusAsync(eventoStatusContrato);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(mensagem, result.Message);
            Assert.Null(result.Error);
            Assert.Null(result.Object);

            _mockContratoRepository.Verify(x => x.CreateAsync(It.IsAny<StatusContrato>()),Times.Once);
        }

        [Fact]
        public async Task CreateContractStatusAsync_Deve_RetornarFailure_QuandoOcorrerUmaFalhaNoCadastro()
        {
            // Arrange 
            var mensagem = "Ocorreu um erro interno no servidor.";
            var eventoStatusContrato = new EventoStatusContrato("000001", "368547", "SUCESSO", "");

            _mockContratoRepository.Setup(x => x.CreateAsync(It.IsAny<StatusContrato>()))
                                                .ThrowsAsync(new Exception(mensagem));
                                   

            var contratoService = new ContratoService(_mockContratoRepository.Object, _mockLogger.Object);

            // Act
            var result = await contratoService.CreateContractStatusAsync(eventoStatusContrato);

            // Assert
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(mensagem, result.Error);
            Assert.Null(result.Message);
            Assert.Null(result.Object);

            _mockContratoRepository.Verify(x => x.CreateAsync(It.IsAny<StatusContrato>()), Times.Once);
        }

        #endregion
    }
}
