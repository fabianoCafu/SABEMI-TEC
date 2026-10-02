using Microsoft.AspNetCore.Mvc;
using Moq;
using SABEMITEC.PagamentoAPI.Controller;
using SABEMITEC.PagamentoAPI.DTO;
using SABEMITEC.PagamentoAPI.Model;
using SABEMITEC.PagamentoAPI.Service;
using static SABEMITEC.Shared.PartnerResult;

namespace SABEMITEC.PagamentoAPI.Test.Controllers
{
    public class PagamentoControllerTest
    {
        private readonly Mock<IEventoBrutoService> _mockEventoBrutoService;
        private readonly PagamentoController _controller;

        public PagamentoControllerTest()
        {
            _mockEventoBrutoService = new Mock<IEventoBrutoService>();
            _controller = new PagamentoController(_mockEventoBrutoService.Object);
        }

        [Fact]
        public async Task Pagamento_Deve_RetornarNotFound404_QuandoPagamentoDtoForNull()
        {
            // Arrange
            PagamentoDto? pagamentoDto = null;

            // Act
            var result = await _controller.Pagamento(pagamentoDto!);
            
            // Assert
            var badRequestResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, badRequestResult.StatusCode);

            _mockEventoBrutoService.Verify(x => x.CreateEventAsync(It.IsAny<EventoBruto>()), Times.Never);
        }

        [Fact]
        public async Task Pagamento_Deve_RetornarBadRequest400_QuandoEventoBrutoRetornarFalha()
        {
            // Arrange 
            var mensagem = "Erro ao cadastrar evento.";
            _mockEventoBrutoService.Setup(x => x.CreateEventAsync(It.IsAny<EventoBruto>()))
                                   .ReturnsAsync(Result<EventoBruto>.Failure(mensagem));

            // Act
            var result = await _controller.Pagamento(new PagamentoDto());

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
            var result = await _controller.Pagamento(new PagamentoDto());

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
            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.Pagamento(new PagamentoDto()));

            // Assert
            Assert.Equal(mensagem, exception.Message);
            _mockEventoBrutoService.Verify(x => x.CreateEventAsync(It.IsAny<EventoBruto>()), Times.Once);
        }
    }
}
