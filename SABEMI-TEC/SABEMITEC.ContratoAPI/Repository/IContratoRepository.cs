using SABEMITEC.ContratoAPI.Models;
using static SABEMITEC.Shared.PartnerResult;

namespace SABEMITEC.ContratoAPI.Repository
{
    public interface IContratoRepository
    {
        Task<Result<List<StatusContrato>>> GetListContractAsync();
        Task<Result<StatusContrato>> CreateAsync(StatusContrato eventoBruto);
        Task<Result<StatusContrato>> UpdateAsync(StatusContrato eventoBruto);
        Task<Result<Boolean>> ExistsAsync(string idTransacao, string idContrato);
    }
}
