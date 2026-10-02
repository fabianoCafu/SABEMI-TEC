using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using SABEMITEC.PagamentoAPI.DTO;
using SABEMITEC.PagamentoAPI.Model;
using SABEMITEC.PagamentoAPI.Repository;
using SABEMITEC.PagamentoAPI.Service;
using System.Text.Json;
using static SABEMITEC.Shared.PartnerResult;

namespace SABEMITEC.PagamentoAPI.Test.Service
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
        public async void CreateEventAsync_Deve_RetornarIsSuccess_QuandoEventoForPersistidoComSucesso()
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
        public async void CreateEventAsync_Deve_RetornarIsFailure_QuandoPersistirEventoRetornarFalha()
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
        public async void CreateEventAsync_Deve_RetornarIsFailure_QuandoIdTrancaoOuIdContratoNaoForemValidos()
        {
            // Arrange
            var mensagem = "Os atributos 'id_transacao' e 'id_contrato' são obrigaórios!";

            var pagamentoDto = new PagamentoDto
            {
                IdTransacao = string.Empty,
                IdContrato = string.Empty,
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
        public async void CreateEventAsync_Deve_RetornarIsFailure_QuandoOhPagamentoJaExistirNoBancoDeDados()
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

        [Fact]
        public async void CreateEventAsync_Deve_RetornarIsFailure_QuandoUmaExcecaoForGeradaAoCriaUmNovoEvento()
        {
            // Arrange
            var eventoBruto = new EventoBruto();
            var mensagem = "Ocorreu um erro interno no servidor.";
            
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
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(mensagem, result.Error);
            Assert.Null(result.Message);
            Assert.Null(result.Object);

        }

        [Fact]
        public void ValidatePayload_DeveRetornarFalha_QuandoValorForMenorOuIgualAZero()
        {
            // Arrange
            var mensagem = "O atributo 'valor' deve ser maior que 0!";
            var json = "{ \"valor\": 0, \"data_pagamento\": \"2026-03-02\", \"status\": \"PARCELAMENTO\" }";
            using var payload = JsonDocument.Parse(json);

            var eventoBrutoService = new EventoBrutoService(_mockEventoBrutoRepository.Object, _mockSendEndpointProvider.Object, _mockLogger.Object);
            var metodoPrivado = typeof(EventoBrutoService).GetMethod("ValidatePayload", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var resultado = (Result<bool>)metodoPrivado!.Invoke(eventoBrutoService, new object[] { payload })!;

            // Assert
            Assert.False(resultado!.IsSuccess);
            Assert.Equal(mensagem, resultado.Error);
        }

        [Fact]
        public void ValidatePayload_DeveRetornarFalha_QuandoAhDataPagamentoNaoForInformada()
        {
            // Arrange
            var mensagem = "O atributo 'data_pagamento' é obrigatório!";
            var json = "{ \"valor\": 200.50, \"data_pagamento\": null, \"status\": \"QUITAÇÃO\" }";
            using var payload = JsonDocument.Parse(json);

            var eventoBrutoService = new EventoBrutoService(_mockEventoBrutoRepository.Object, _mockSendEndpointProvider.Object, _mockLogger.Object);
            var metodoPrivado = typeof(EventoBrutoService).GetMethod("ValidatePayload", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var resultado = (Result<bool>)metodoPrivado!.Invoke(eventoBrutoService, new object[] { payload })!;

            // Assert
            Assert.False(resultado!.IsSuccess);
            Assert.Equal(mensagem, resultado.Error);
        }

        [Fact]
        public void ValidatePayload_DeveRetornarFalha_QuandoOhStatusNaoForInformada()
        {
            // Arrange
            var mensagem = "O atributo 'status' é obrigatório!";
            var json = "{ \"valor\": 980.55, \"data_pagamento\": \"\", \"status\": null }";
            using var payload = JsonDocument.Parse(json);

            var eventoBrutoService = new EventoBrutoService(_mockEventoBrutoRepository.Object, _mockSendEndpointProvider.Object, _mockLogger.Object);
            var metodoPrivado = typeof(EventoBrutoService).GetMethod("ValidatePayload", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var resultado = (Result<bool>)metodoPrivado!.Invoke(eventoBrutoService, new object[] { payload })!;

            // Assert
            Assert.False(resultado!.IsSuccess);
            Assert.Equal(mensagem, resultado.Error);
        }

        [Fact]
        public void ValidatePayload_DeveRetornarFalha_QuandoForGeradaUmaExcecaoAoValidarPayload()
        {
            // Arrange 
            var mensagem = "Ocorreu um erro interno no servidor.";
            var json = "{ \"data_pagamento\": \"2026-03-02\", \"status\": \"Pago\" }";
            using var payload = JsonDocument.Parse(json);

            var eventoBrutoService = new EventoBrutoService(_mockEventoBrutoRepository.Object, _mockSendEndpointProvider.Object, _mockLogger.Object);
            var metodoPrivado = typeof(EventoBrutoService).GetMethod("ValidatePayload", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var resultado = (Result<bool>)metodoPrivado!.Invoke(eventoBrutoService, new object[] { payload })!;

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(mensagem, resultado.Error);
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