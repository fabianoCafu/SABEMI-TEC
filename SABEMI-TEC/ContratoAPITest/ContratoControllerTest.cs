using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SABEMITEC.ContratoAPI.Controllers;
using SABEMITEC.ContratoAPI.Models;
using SABEMITEC.ContratoAPI.Service;
using static SABEMITEC.Shared.PartnerResult;

namespace SABEMITEC.ContratoAPI.Test.Controlles
{
    public class ContratoControllerTest
    {
        private readonly Mock<IContratoService> _mockContratoService;
        private readonly Mock<ILogger<ContratoController>> _mockLogger;
        private readonly ContratoController _controller;

        public ContratoControllerTest()
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
        public async void PagamentosProcessados_Deve_Retornar_NotFound404_QuandoNaoExistirNenhumStatusContratoCadastrado()
        {
            // Arrange
             var mensagem = "Não existe nemhum pagamento Processado!";
            _mockContratoService.Setup(a => a.GetListContractAsync())
                                .ReturnsAsync(Result<List<StatusContrato>>.Success(mensagem));

            // Act
            var result = await _controller.PagamentosProcessados();

            // Assert
            var createResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, createResult.StatusCode);
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
        public async Task Pagamento_Deve_Retornar500_QuandoOcorrerUmaException()
        {
            // Arrange
            var mensagem = "Ocorreu um erro interno no servidor.";
            _mockContratoService.Setup(x => x.GetListContractAsync())
                                .ThrowsAsync(new Exception(mensagem));

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.PagamentosProcessados());

            // Assert
            Assert.Equal(mensagem, exception.Message);
            _mockContratoService.Verify(x => x.GetListContractAsync(), Times.Once);
        }

        #endregion
    }
}


