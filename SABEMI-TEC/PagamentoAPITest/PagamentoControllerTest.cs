using Moq;
using Microsoft.AspNetCore.Mvc;
using SABEMITEC.PagamentoAPI.DTO;
using Microsoft.Extensions.Logging;
using SABEMITEC.PagamentoAPI.Model;
using SABEMITEC.PagamentoAPI.Service;
using SABEMITEC.PagamentoAPI.Controller;
using static SABEMITEC.Shared.PartnerResult;

namespace PagamentoAPITest
{
    public class PagamentoControllerTest
    {
        private readonly Mock<IEventoBrutoService> _mockEventoBrutoService;
        private readonly Mock<ILogger<PagamentoController>> _mockLogger;
        private readonly PagamentoController _controller;

        public PagamentoControllerTest()
        {
            _mockEventoBrutoService = new Mock<IEventoBrutoService>();
            _mockLogger = new Mock<ILogger<PagamentoController>>();
            _controller = new PagamentoController(_mockEventoBrutoService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Pagamento_Deve_RetornarBadRequest400_QuandoPagamentoDtoForNull()
        {
            // Arrange
            PagamentoDTO? pagamentoDto = null;

            // Act
            var result = await _controller.Pagamento(pagamentoDto!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            _mockEventoBrutoService.Verify(x => x.CreateEventAsync(It.IsAny<EventoBruto>()), Times.Never);
        }

        [Fact]
        public async Task Pagamento_Deve_RetornarBadRequest400_QuandoEventoBrutoRetornarFalha()
        {
            // Arrange 
            string mensagem = "Erro ao cadastrar evento.";

            _mockEventoBrutoService.Setup(x => x.CreateEventAsync(It.IsAny<EventoBruto>()))
                                   .ReturnsAsync(Result<EventoBruto>.Failure(mensagem));

            // Act
            var result = await _controller.Pagamento(new PagamentoDTO());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal(mensagem, badRequestResult.Value);

            _mockEventoBrutoService.Verify(x => x.CreateEventAsync(It.IsAny<EventoBruto>()), Times.Once);
        }


        [Fact]
        public async Task Pagamento_Deve_RetornarOk200_QuandoEventoForCadastradoComSucesso()
        {
            // Arrange
            var mensagem = "Cadastro realizado com Sucesso!";
            _mockEventoBrutoService.Setup(x => x.CreateEventAsync(It.IsAny<EventoBruto>()))
                                   .ReturnsAsync(Result<EventoBruto>.Success(mensagem));

            // Act
            var result = await _controller.Pagamento(new PagamentoDTO());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(mensagem, okResult.Value);

            _mockEventoBrutoService.Verify(x => x.CreateEventAsync(It.IsAny<EventoBruto>()), Times.Once);
        }

        [Fact]
        public async Task Pagamento_Deve_Retornar500_QuandoOcorrerUmaException()
        {
            // Arrange 
            var mensagem = "Ocorreu um erro interno no servidor.";
            _mockEventoBrutoService.Setup(x => x.CreateEventAsync(It.IsAny<EventoBruto>()))
                                   .ThrowsAsync(new Exception(mensagem));

            // Act
            var result = await _controller.Pagamento(new PagamentoDTO());

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal(mensagem, statusCodeResult.Value);
            _mockEventoBrutoService.Verify(x => x.CreateEventAsync(It.IsAny<EventoBruto>()),Times.Once);
        }
    }
}
