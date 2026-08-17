using SABEMITEC.PagamentoAPI.Model;
using static SABEMITEC.Shared.PartnerResult;

namespace SABEMITEC.PagamentoAPI.Repository
{
    public interface IEventoBrutoRepository
    {
        Task<Result<EventoBruto>> CreateAsync(EventoBruto eventoBruto);
        Task<Result<Boolean>> ExistsEventAsync(string idTransacao);
    }
}
