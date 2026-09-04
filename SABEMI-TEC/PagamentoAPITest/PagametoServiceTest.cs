using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using SABEMITEC.PagamentoAPI.DTO;
using SABEMITEC.PagamentoAPI.Model;
using SABEMITEC.PagamentoAPI.Repository;
using SABEMITEC.PagamentoAPI.Service;
using System.Text.Json;
using static SABEMITEC.Shared.PartnerResult;

namespace PagamentoAPITest
{
    public class PagametoServiceTest
    {
        private readonly Mock<IEventoBrutoRepository> _mockEventoBrutoRepository;
        private readonly Mock<ISendEndpointProvider> _mockSendEndpointProvider;
        private readonly Mock<ISendEndpoint> _mockSendEndpoint;
        private readonly Mock<ILogger<EventoBrutoService>> _mockLogger;

        public PagametoServiceTest()
        {
            _mockEventoBrutoRepository = new Mock<IEventoBrutoRepository>();
            _mockSendEndpointProvider = new Mock<ISendEndpointProvider>();
            _mockSendEndpoint = new Mock<ISendEndpoint>();
            _mockLogger = new Mock<ILogger<EventoBrutoService>>();
        }

        [Fact]
        public async void CreateEventAsync_Deve_RetornarSucesso_QuandoEventoForPersistidoComSucesso()
        {
            // Arrange
            var mensagem = "Evento Cadastrado com Sucesso!";
            //var pagamentoDto = PagamentoDtoPayloadComErro();
            var pagamentoDto = PagamentoDtoPayloadComSucesso();
            var eventoBruto = DefinirEventoBruto(pagamentoDto);

            _mockEventoBrutoRepository.Setup(x => x.CreateAsync(It.IsAny<EventoBruto>()))
                                      .ReturnsAsync(Result<EventoBruto>.Success(eventoBruto));

            _mockEventoBrutoRepository.Setup(x => x.ExistsEventAsync(It.IsAny<string>()))
                                      .ReturnsAsync(Result<bool>.Failure("Evento não encontrado"));

            _mockSendEndpointProvider.Setup(x => x.GetSendEndpoint(It.IsAny<Uri>()))
                                     .ReturnsAsync(_mockSendEndpoint.Object);

            var eventoBrutoService = new EventoBrutoService(_mockEventoBrutoRepository.Object, _mockSendEndpointProvider.Object, _mockLogger.Object);

            // Act
            var result = await eventoBrutoService.CreateEventAsync(eventoBruto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(mensagem, result.Message);
        }

        [Fact]
        public async void CreateEventAsync_Deve_RetornarFailure_QuandoPersistirEventoRetornarFalha()
        {
            // Arrange
            var mensagem = "Erro ao persistir o evento.";
            var pagamentoDto = PagamentoDtoPayloadComSucesso();
            var eventoBruto = DefinirEventoBruto(pagamentoDto);
          

            _mockEventoBrutoRepository.Setup(x => x.CreateAsync(It.IsAny<EventoBruto>()))
                                      .ReturnsAsync(Result<EventoBruto>.Failure(mensagem));

            _mockEventoBrutoRepository.Setup(x => x.ExistsEventAsync(It.IsAny<string>()))
                                      .ReturnsAsync(Result<bool>.Failure("Evento não encontrado"));

            _mockSendEndpointProvider.Setup(x => x.GetSendEndpoint(It.IsAny<Uri>()))
                                     .ReturnsAsync(_mockSendEndpoint.Object);

            var eventoBrutoService = new EventoBrutoService(_mockEventoBrutoRepository.Object, _mockSendEndpointProvider.Object, _mockLogger.Object);

            // Act
            var result = await eventoBrutoService.CreateEventAsync(eventoBruto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Message);
            Assert.Null(result.Object);
            Assert.Equal(mensagem, result.Error);
        }

        [Fact]
        public async void CreateEventAsync_Deve_RetornarFailure_QuandoIdTrancaoOuIdContratoNaoForemValidos()
        {
            // Arrange
            var mensagem = "Os atributos 'id_transacao' e 'id_contrato' são obrigaórios!";

            var pagamentoDto = new PagamentoDto
            {
                IdTransacao = "",
                IdContrato = "",
                Valor = 100.00M,
                DataPagamento = DateTime.Now,
                Status = "PARCELAMENTO"
            };

            var eventoBruto = DefinirEventoBruto(pagamentoDto);

            _mockEventoBrutoRepository.Setup(x => x.CreateAsync(It.IsAny<EventoBruto>()))
                                      .ReturnsAsync(Result<EventoBruto>.Failure(mensagem));

            _mockEventoBrutoRepository.Setup(x => x.ExistsEventAsync(It.IsAny<string>()))
                                      .ReturnsAsync(Result<bool>.Failure("tESTE"));

            _mockSendEndpointProvider.Setup(x => x.GetSendEndpoint(It.IsAny<Uri>()))
                                     .ReturnsAsync(_mockSendEndpoint.Object);

            var eventoBrutoService = new EventoBrutoService(_mockEventoBrutoRepository.Object, _mockSendEndpointProvider.Object, _mockLogger.Object);

            // Act
            var result = await eventoBrutoService.CreateEventAsync(eventoBruto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Message);
            Assert.Null(result.Object);
            Assert.Equal(mensagem, result.Error);
        }

        [Fact]
        public async void CreateEventAsync_Deve_RetornarFailure_QuandoOhPagamentoJaExistirNoBancoDeDados()
        {
            // Arrange
            var mensagem = "Pagamento já Processado!";
            var pagamentoDto = PagamentoDtoPayloadComSucesso();
            var eventoBruto = DefinirEventoBruto(pagamentoDto);

            _mockEventoBrutoRepository.Setup(x => x.CreateAsync(It.IsAny<EventoBruto>()))
                                      .ReturnsAsync(Result<EventoBruto>.Failure(mensagem));

            _mockEventoBrutoRepository.Setup(x => x.ExistsEventAsync(It.IsAny<string>()))
                                    .ReturnsAsync(Result<bool>.Success(true));

            _mockSendEndpointProvider.Setup(x => x.GetSendEndpoint(It.IsAny<Uri>()))
                                     .ReturnsAsync(_mockSendEndpoint.Object);

            var eventoBrutoService = new EventoBrutoService(_mockEventoBrutoRepository.Object, _mockSendEndpointProvider.Object, _mockLogger.Object);

            // Act
            var result = await eventoBrutoService.CreateEventAsync(eventoBruto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Message);
            Assert.Null(result.Object);
            Assert.Equal(mensagem, result.Error);
        }

        #region Metodos Privates
        private static PagamentoDto PagamentoDtoPayloadComSucesso()
        {
            return new PagamentoDto
            {
                IdTransacao = "000001",
                IdContrato = "000365",
                Valor = 100.00M,
                DataPagamento = DateTime.Now,
                Status = "PARCELAMENTO"
            };
        }

        private static PagamentoDto PagamentoDtoPayloadComErro()
        {
            return new PagamentoDto
            {
                IdTransacao = "000001",
                IdContrato = "000365",
                Valor = 0,
                DataPagamento = DateTime.Now,
                Status = "PARCELAMENTO"
            };
        }

        private static EventoBruto DefinirEventoBruto(PagamentoDto pagamentoDto)
        {
            return new EventoBruto()
            {
                Id = new Guid(),
                Payload = JsonSerializer.Serialize(pagamentoDto).ToString(),
                DataRecebimento = DateTime.Now
            };
        }

        #endregion
    }
}