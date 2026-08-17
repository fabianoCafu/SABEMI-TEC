using SABEMITEC.PagamentoAPI.Model;
using static SABEMITEC.Shared.PartnerResult;

namespace SABEMITEC.PagamentoAPI.Service
{
    public interface IEventoBrutoService
    {
        Task<Result<EventoBruto>> CreateEventAsync(EventoBruto eventoBruto); 
    }
}
