using Moq;
using Microsoft.AspNetCore.Mvc;
using SABEMITEC.ContratoAPI.Models;
using Microsoft.Extensions.Logging;
using SABEMITEC.ContratoAPI.Service;
using SABEMITEC.ContratoAPI.Controllers;
using static SABEMITEC.Shared.PartnerResult;

namespace ContratoAPITest
{
    public class ContratoControllerTests
    {
        private readonly Mock<IContratoService> _mockContratoService;
        private readonly Mock<ILogger<ContratoController>> _mockLogger;
        private readonly ContratoController _controller;

        public ContratoControllerTests()
        {
            _mockContratoService = new Mock<IContratoService>();
            _mockLogger = new Mock<ILogger<ContratoController>>();
            _controller = new ContratoController(_mockContratoService.Object, _mockLogger.Object);
        }

        #region EndPointt pagamentos-processados

        [Fact]
        public async void PagamentosProcessados_Deve_Retornar_OK200_QuandoOsContratosForemListadosComSucesso()
        {
            // Arrange
            var contratos = new List<StatusContrato>();
            var serviceResult = Result<List<StatusContrato>>.Success(contratos);

            _mockContratoService.Setup(x => x.GetListContractAsync())
                                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.PagamentosProcessados();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Same(serviceResult, okResult.Value);

            _mockContratoService.Verify(x => x.GetListContractAsync(), Times.Once);
        }

        [Fact]
        public async void PagamentosProcessados_Deve_Retornar_OK200_QuandoNaoExistirNenhumStatusContratoCadastrado()
        {
            // Arrange
             var mensagem = "Não existe nemhum pagamento Processado!";
            _mockContratoService.Setup(a => a.GetListContractAsync())
                                .ReturnsAsync(Result<List<StatusContrato>>.Success(mensagem));

            // Act
            var result = await _controller.PagamentosProcessados();

            // Assert
            var createResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, createResult.StatusCode);
            Assert.Equal(mensagem, createResult.Value);

            _mockContratoService.Verify(x => x.GetListContractAsync(), Times.Once);
        }

        [Fact]
        public async Task PagamentosProcessados_Deve_RetornarUmBadRequest400_QuandoOhRetornoForUmaFalha()
        {
            // Arrange
            var mensagem = "Ocorreu um erro interno no servidor.";
           _mockContratoService.Setup(x => x.GetListContractAsync())
                                .ReturnsAsync(Result<List<StatusContrato>>.Failure(mensagem));

            // Act
            var result = await _controller.PagamentosProcessados();

            // Assert
            var statusCodeResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
            Assert.Equal(mensagem, statusCodeResult.Value);

            _mockContratoService.Verify(x => x.GetListContractAsync(), Times.Once);
        }

        [Fact]
        public async Task PagamentosProcessados_Deve_Retornar_InternalServerError500_QuandoOcorrerUmaException()
        {
            // Arrange
            _mockContratoService.Setup(x => x.GetListContractAsync())
                                .ThrowsAsync(new Exception("Internal Server Error"));

            // Act
            var result = await _controller.PagamentosProcessados();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Ocorreu um erro interno no servidor.", statusCodeResult.Value);

            _mockContratoService.Verify(x => x.GetListContractAsync(), Times.Once);
        }

        #endregion
    }
}


