using Microsoft.AspNetCore.Mvc;
using webApi.Service;

namespace webApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CotizacionController : ControllerBase
    {
        private readonly CriptoYaService _criptoService;

        public CotizacionController(CriptoYaService criptoService)
        {
            _criptoService = criptoService;
        }

        [HttpGet("{abreviatura}")]
        public async Task<IActionResult> Get(string abreviatura)
        {
            try
            {
                var cotizacion = await _criptoService.ObtenerCotizacion(abreviatura);
                return Ok(cotizacion);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
