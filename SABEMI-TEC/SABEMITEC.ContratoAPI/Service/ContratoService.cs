using SABEMITEC.ContratoAPI.Models;
using SABEMITEC.ContratoAPI.Repository;
using SABEMITEC.Shared.Event;
using static SABEMITEC.Shared.PartnerResult;

namespace SABEMITEC.ContratoAPI.Service
{
    public class ContratoService :IContratoService
    {
        private readonly IContratoRepository _contratoRepository;
        private readonly ILogger<ContratoService> _logger;

        public ContratoService(
            IContratoRepository contratoRepository,
            ILogger<ContratoService> logger)
        {
            _contratoRepository = contratoRepository ?? throw new ArgumentNullException(nameof(contratoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); 
        }

        public async Task<Result<List<StatusContrato>>> GetListContractAsync()
        {
            try
            {
                var result = await _contratoRepository.GetListContractAsync();

                if(result.Object!.Any())
                {
                    return Result<List<StatusContrato>>.Success(result.Object);
                }
                else
                {
                    return Result<List<StatusContrato>>.Success("Não existe nemhum pagamento Processado!");
                } 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar o StatusContrato.");
                return Result<List<StatusContrato>>.Failure("Ocorreu um erro interno no servidor.");
            }
        }

        public async Task<Result<EventoStatusContrato>> CreateContractStatusAsync(EventoStatusContrato eventoStatusContrato)
        {
            try
            {
                var statusContrato = new StatusContrato(eventoStatusContrato.IdTransacao!, eventoStatusContrato.IdContrato!, eventoStatusContrato.Status!, eventoStatusContrato.Falha!);
                await _contratoRepository.CreateAsync(statusContrato!);

                return Result<EventoStatusContrato>.Success("StatusContrato cadastrado com Sucesso!");  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar o StatusContrato.");
                return Result<EventoStatusContrato>.Failure("Ocorreu um erro interno no servidor.");
            }
        }
    }
}
