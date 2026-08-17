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
        private readonly ILogger<PagamentoController> _logger;

        public PagamentoController(
            IEventoBrutoService eventoBrutoService,
            ILogger<PagamentoController> logger)
        {
            _eventoBrutoService = eventoBrutoService ?? throw new ArgumentNullException(nameof(eventoBrutoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("pagamento")] 
        public async Task<IActionResult> Pagamento([FromBody] PagamentoDTO pagamentoDto)
        {
            try
            {
                if (pagamentoDto is null)
                {
                    return BadRequest();
                }

                var eventoBruto = new EventoBruto(pagamentoDto);
                var result = await _eventoBrutoService.CreateEventAsync(eventoBruto);

                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }
                else
                {
                    return Ok("Cadastro realizado com Sucesso!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao receber evento de pagamento.");
                return StatusCode(500, "Ocorreu um erro interno no servidor."); 
            }
        }
    }
}
