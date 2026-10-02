using Microsoft.AspNetCore.Mvc;
using SABEMITEC.PagamentoAPI.DTO;
using SABEMITEC.PagamentoAPI.Model;
using SABEMITEC.PagamentoAPI.Service;

namespace SABEMITEC.PagamentoAPI.Controller
{
    [ApiController]
    [Route("webhooks")]
    public class PagamentoController : ControllerBase
    {
        private readonly IEventoBrutoService _eventoBrutoService;
        
        public PagamentoController(IEventoBrutoService eventoBrutoService)
        {
            _eventoBrutoService = eventoBrutoService;
        }

        [HttpPost("pagamento")] 
        public async Task<IActionResult> Pagamento([FromBody] PagamentoDto pagamentoDto)
        {
            if (pagamentoDto is null)
            {
                return NotFound("Dados do Pagamento não informado!");
            }

            var result = await _eventoBrutoService.CreateEventAsync(new EventoBruto(pagamentoDto));

            return (result.IsFailure) 
                ? BadRequest(result.Error) 
                : Ok("Cadastro realizado com Sucesso!");
        }
    }
}
