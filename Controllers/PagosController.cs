using BioLabApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BioLabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class PagosController : ControllerBase
    {
        private readonly IPagosService _pagosService;

        public PagosController(IPagosService pagosService)
        {
            _pagosService = pagosService;
        }


        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    var result = await _pagosService.GetAllPagosAsync();
        //    if (!result.Success) return NotFound(result);
        //    return Ok(result);
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _pagosService.GetPagoByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("metodo/{idMetodo}")]
        public async Task<IActionResult> GetByMetodo(int idMetodo)
        {
            var result = await _pagosService.GetPagosByMetodoAsync(idMetodo);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("orden/{ordenId}")]
        public async Task<IActionResult> GetByOrden(int ordenId)
        {
            var result = await _pagosService.GetPagosByOrdenAsync(ordenId);
            if (!result.Success) return NotFound(result);
            return Ok(result);

        }

        [HttpGet("referencia/{referenciaId}")]
        public async Task<IActionResult> GetByReferencia(string referenciaId)
        {
            var result = await _pagosService.GetPagoByReferenciaAsync(referenciaId);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("fechas")]
        public async Task<IActionResult> GetByFechas([FromBody] DateTime? fechaInicio, DateTime? fechaFin)
        {
            var result = await _pagosService.GetAllPagosEntreFechasAsync(fechaInicio, fechaFin);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PagosModel pago)
        {
            var result = await _pagosService.CreatePagoAsync(pago);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PagosModel pago)
        {
            var result = await _pagosService.UpdatePagoAsync(pago, id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromBody] int AdminId)
        {
            var result = await _pagosService.AnulatePagosAsync(id, AdminId);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

    }
}
