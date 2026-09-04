using MassTransit;
using SABEMITEC.PagamentoAPI.DTO;
using SABEMITEC.PagamentoAPI.Model;
using SABEMITEC.PagamentoAPI.Repository;
using SABEMITEC.PagamentoAPI.Util;
using SABEMITEC.Shared.Event;
using System.Text.Json;
using static SABEMITEC.PagamentoAPI.Util.EnumPagamento;
using static SABEMITEC.Shared.PartnerResult;

namespace SABEMITEC.PagamentoAPI.Service
{
    public class EventoBrutoService : IEventoBrutoService
    {
        private readonly IEventoBrutoRepository _eventoBrutoRepository;
        private readonly ILogger<EventoBrutoService> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public EventoBrutoService(
            IEventoBrutoRepository eventoBrutoRepository,
            ISendEndpointProvider sendEndpointProvider,
            ILogger<EventoBrutoService> logger)
        {
            _eventoBrutoRepository = eventoBrutoRepository; 
            _sendEndpointProvider = sendEndpointProvider;
            _logger = logger;
        }

        public async Task<Result<EventoBruto>> CreateEventAsync(EventoBruto eventoBruto)
        {
            try
            {    
                var result = await PersistEventAsync(eventoBruto);

                if (result.IsSuccess)
                {
                    return Result<EventoBruto>.Success("Evento Cadastrado com Sucesso!");
                }
                else
                { 
                    return Result<EventoBruto>.Failure(result.Error!);
                } 
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar um EventoBruto.");
                return Result<EventoBruto>.Failure("Ocorreu um erro interno no servidor.");
            }
        }

        #region Metodo Private 

        private async Task<Result<Boolean>> PersistEventAsync(EventoBruto grossEvent)
        {
            var payment = JsonSerializer.Deserialize<PagamentoDto>(JsonDocument.Parse(grossEvent.Payload));
            var eventoExiste = await _eventoBrutoRepository.ExistsEventAsync(payment!.IdTransacao!);

            if (eventoExiste.IsSuccess)
            {
                return Result<bool>.Failure("Pagamento já Processado!");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(payment.IdTransacao) || string.IsNullOrWhiteSpace(payment.IdContrato))
                {
                    return Result<bool>.Failure("Os atributos 'id_transacao' e 'id_contrato' são obrigaórios!");
                }

                var payload = ValidatePayload(JsonDocument.Parse(grossEvent.Payload)); 
                var newGrossEvent = await _eventoBrutoRepository.CreateAsync(grossEvent);

                if (newGrossEvent.IsSuccess)
                {
                    await PublishRabbitMQMessage(payload, payment);
                    return Result<bool>.Success(true);
                }
                else
                {
                    return Result<bool>.Failure(newGrossEvent.Error!);
                }   
            }
        }

        private async Task PublishRabbitMQMessage(
            Result<bool> payload,
            PagamentoDto payment)
        {
            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:status-contrato"));

            if (payload.IsFailure)
            {
                await endpoint.Send(new EventoStatusContrato(payment.IdTransacao!, payment.IdContrato!, StatusContrato.Erro.GetDescription(), payload.Error));
            }
            else
            {
                await endpoint.Send(new EventoStatusContrato(payment.IdTransacao!, payment.IdContrato!, StatusContrato.Sucesso.GetDescription()));
            }
        }

        private Result<bool> ValidatePayload(JsonDocument payload)
        {
            try
            {
                if (payload.RootElement.GetProperty("valor").GetDecimal() <= 0)
                {
                    return Result<bool>.Failure("O atributo 'valor' deve ser maior que 0!");
                }

                var dataPagamento = payload.RootElement.GetProperty("data_pagamento");

                if (dataPagamento.ValueKind == JsonValueKind.Null || dataPagamento.ValueKind == JsonValueKind.Undefined)
                {
                    return Result<bool>.Failure("O atributo 'data_pagamento' é obrigatório!");
                }

                if (string.IsNullOrWhiteSpace(payload.RootElement.GetProperty("status").GetString()))
                {
                    return Result<bool>.Failure("O atributo 'status' é obrigatório!");
                }

                return Result<bool>.Success(true);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar Payload."); 
                return Result<bool>.Failure("Ocorreu um erro interno no servidor.");
            }
        }

        #endregion

    }
}
