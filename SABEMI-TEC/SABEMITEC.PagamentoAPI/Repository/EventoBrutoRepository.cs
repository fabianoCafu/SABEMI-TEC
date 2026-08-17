using Microsoft.EntityFrameworkCore;
using SABEMITEC.PagamentoAPI.Context;
using SABEMITEC.PagamentoAPI.Model;
using static SABEMITEC.Shared.PartnerResult;

namespace SABEMITEC.PagamentoAPI.Repository
{
    public class EventoBrutoRepository : IEventoBrutoRepository
    {
        private readonly SQLSeverContext _context;
        private readonly ILogger<EventoBrutoRepository> _logger;

        public EventoBrutoRepository(
            SQLSeverContext context,
            ILogger<EventoBrutoRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<EventoBruto>> CreateAsync(EventoBruto eventoBruto)
        {
            try
            {
                 _context.LogEventosBruto!.Add(eventoBruto);
                await _context.SaveChangesAsync();

                return Result<EventoBruto>.Success(eventoBruto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastra um Evento Bruto no banco.");
                return Result<EventoBruto>.Failure("Erro interno ao receber o evento.");  
            }
        }

        public async Task<Result<Boolean>> ExistsEventAsync(string idTransacao)
        {
            try
            {
                var paymentExists = await _context.LogEventosBruto!
                                                  .FromSqlInterpolated(GetEventoBrutoByIdTransacaoQuery(idTransacao))
                                                  .AsNoTracking()
                                                  .CountAsync();

                return (paymentExists > 0) 
                    ? Result<bool>.Success(true) 
                    : Result<bool>.Failure(string.Empty);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar se existe evento.");
                return Result<bool>.Failure("Erro interno ao validar se existe evento.");
            }
        }

        #region Metodo Private

        private static FormattableString GetEventoBrutoByIdTransacaoQuery(string idTransacao)
        {
            return $@"SELECT 
                            *
                       FROM LogEventosBruto 
                            WHERE JSON_VALUE(Payload, '$.id_transacao') = {idTransacao}";
        }

        #endregion

    }
}
