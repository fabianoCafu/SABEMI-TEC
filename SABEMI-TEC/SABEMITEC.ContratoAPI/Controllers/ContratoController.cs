using Microsoft.AspNetCore.Mvc;
using SABEMITEC.ContratoAPI.Service;

namespace SABEMITEC.ContratoAPI.Controllers
{
    [ApiController]
    public class ContratoController : Controller
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
            try
            {
                var result = await _contratoService.GetListContractAsync();

                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }
                else
                {
                    if (result.Object is null)
                    {
                        return Ok(result.Message);
                    }
                    else
                    {
                        return Ok(result);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao lista contratos.");
                return StatusCode(500, "Ocorreu um erro interno no servidor.");
            }
        }
    }
}