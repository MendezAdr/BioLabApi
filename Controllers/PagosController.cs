using BioLabApi.Services.Interfaces;
using BioLabApi.Models;
using Microsoft.AspNetCore.Mvc;
using BioLabApi.Models.DTOs;

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

        // CORRECCIÓN: Usar [FromQuery] en lugar de [FromBody] para un GET
        [HttpGet("fechas")]
        public async Task<IActionResult> GetByFechas([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
        {
            var result = await _pagosService.GetAllPagosEntreFechasAsync(fechaInicio, fechaFin);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        // CORRECCIÓN: Añadido el Header de seguridad
        [HttpPost]
        public async Task<IActionResult> CreateAddPago([FromBody] PagoStandaloneCreateDTO pago, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
        {
            var result = await _pagosService.CreateAddPagoAsync(pago, usuarioId); // Asumo que actualizarás la interfaz del servicio para recibir el id
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        // CORRECCIÓN: Añadido el Header de seguridad
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PagoUpdateDTO pago, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
        {
            var result = await _pagosService.UpdatePagoAsync(pago, id, usuarioId); // Asumo actualización de la interfaz
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        // CORRECCIÓN: Cambiado [FromBody] a [FromHeader] y renombrado a usuarioId
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
        {
            var result = await _pagosService.AnulatePagosAsync(id, usuarioId);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
    }
}