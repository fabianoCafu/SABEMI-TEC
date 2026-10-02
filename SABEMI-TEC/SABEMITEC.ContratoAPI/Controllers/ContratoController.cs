using Microsoft.AspNetCore.Mvc;
using SABEMITEC.ContratoAPI.Service;

namespace SABEMITEC.ContratoAPI.Controllers
{
    [ApiController]
    [Route("contratos")]
    public class ContratoController : ControllerBase
    {
        private readonly IContratoService _contratoService;
        
        public ContratoController(IContratoService contratoService)
        {
            _contratoService = contratoService; 
        }

        [HttpGet("pagamentos-processados")]
        public async Task<IActionResult> PagamentosProcessados()
        { 
            var result = await _contratoService.GetListContractAsync();

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }
            
            return (result.Object is null) 
                ? NotFound(result.Message) 
                : Ok(result); 
        }
    }
}