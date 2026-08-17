using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SABEMITEC.ContratoAPI.Context;
using SABEMITEC.ContratoAPI.Models;
using SABEMITEC.ContratoAPI.SignalR;
using static SABEMITEC.Shared.PartnerResult;

namespace SABEMITEC.ContratoAPI.Repository
{
    public class ContratoRepository : IContratoRepository
    {
        private readonly IHubContext<PagamentoHub> _hubContext;
        private readonly SQLSeverContext _context;
        private readonly ILogger<ContratoRepository> _logger;

        public ContratoRepository(
            IHubContext<PagamentoHub> hubContext,
            SQLSeverContext context,
            ILogger<ContratoRepository> logger)
        {
            _hubContext = hubContext;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<List<StatusContrato>>> GetListContractAsync()
        {
            try
            {
                var statusContratos = await _context.StatusContrato!.AsNoTracking()
                                                                    .OrderByDescending(x => x.DataProcessamento)
                                                                    .ToListAsync();

                return Result<List<StatusContrato>>.Success(statusContratos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar um StatusContrato.");
                return Result<List<StatusContrato>>.Failure("Erro interno ao lista  Status do Contrato.");
            }
        }

        public async Task<Result<StatusContrato>> CreateAsync(StatusContrato statusContrato)
        {
            try
            {
                _context.StatusContrato!.Add(statusContrato);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("PagamentoAtualizado");

                return Result<StatusContrato>.Success(statusContrato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar um StatusContrato.");
                return Result<StatusContrato>.Failure("Erro interno ao atualizar o StatusContrato.");
            }
        }

        public async Task<Result<bool>> ExistsAsync(
            string idTransacao,
            string idContrato)
        {
            try
            {
                var contractExists = await _context.StatusContrato!
                                                   .Where(c => c.IdTransacao == idTransacao && c.IdContrato == idContrato)
                                                   .AsNoTracking()
                                                   .CountAsync();

                return (contractExists > 0)
                    ? Result<bool>.Success(true)
                    : Result<bool>.Failure(string.Empty);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar se existe Contrato.");
                return Result<bool>.Failure("Erro interno ao validar se existe Contrato.");
            }
        }
    }
}
