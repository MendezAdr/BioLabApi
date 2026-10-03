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

        // NUEVO ENDPOINT: Para cargar la tabla principal de pagos en React
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _pagosService.GetAllPagosAsync();
            if (!result.Success) return NotFound(result);
            return Ok(result);
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

        [HttpGet("fechas")]
        public async Task<IActionResult> GetByFechas([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
        {
            var result = await _pagosService.GetAllPagosEntreFechasAsync(fechaInicio, fechaFin);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddPago([FromBody] PagoStandaloneCreateDTO pago, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
        {
            var result = await _pagosService.CreateAddPagoAsync(pago, usuarioId); 
            if (!result.Success) return BadRequest(result); // Corregido a BadRequest
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PagoUpdateDTO pago, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
        {
            var result = await _pagosService.UpdatePagoAsync(pago, id, usuarioId); 
            if (!result.Success) return BadRequest(result); // Corregido a BadRequest
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
        {
            var result = await _pagosService.AnulatePagosAsync(id, usuarioId);
            if (!result.Success) return BadRequest(result); // Corregido a BadRequest
            return Ok(result);
        }
    }
}