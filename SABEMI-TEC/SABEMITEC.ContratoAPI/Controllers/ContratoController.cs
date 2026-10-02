using Microsoft.AspNetCore.Mvc;
using SABEMITEC.ContratoAPI.Service;

namespace SABEMITEC.ContratoAPI.Controllers
{
    [ApiController]
    [Route("contratos")]
    public class ContratoController : ControllerBase
    {
        private readonly IContratoService _contratoService;
        private readonly ILogger<ContratoController> _logger;

        public ContratoController(
            IContratoService contratoService,
            ILogger<ContratoController> logger)
        {
            _contratoService = contratoService;
            _logger = logger;  
        }

        [HttpGet("pagamentos-processados")]
        public async Task<IActionResult> PagamentosProcessados()
        { 
            var result = await _contratoService.GetListContractAsync();

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }
            else
            {
                return (result.Object is null) ? NotFound(result.Message) : Ok(result);
            }
        }
    }
}