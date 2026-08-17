using MassTransit;
using SABEMITEC.ContratoAPI.Service;
using SABEMITEC.Shared.Event;

namespace SABEMITEC.ContratoAPI.Consumers
{
    public class EventoStatusContratoConsumer : IConsumer<EventoStatusContrato>
    {
        private readonly IContratoService _contratoService;

        public EventoStatusContratoConsumer(IContratoService contratoService)
        {
            _contratoService = contratoService;
        }

        public async Task Consume(ConsumeContext<EventoStatusContrato> context)
        {
            var evento = context.Message;
            await _contratoService.CreateContractStatusAsync(evento);
        }
    }
}
