using SABEMITEC.ContratoAPI.Models;
using SABEMITEC.Shared.Event;
using static SABEMITEC.Shared.PartnerResult;

namespace SABEMITEC.ContratoAPI.Service
{
    public interface IContratoService
    {
        Task<Result<List<StatusContrato>>> GetListContractAsync();
        Task<Result<EventoStatusContrato>> CreateContractStatusAsync(EventoStatusContrato eventoStatusContrato);
    }
}
