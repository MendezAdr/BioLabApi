using Microsoft.AspNetCore.Mvc;
using BioLabApi.Models;
using BioLabApi.Services.Interfaces;
using BioLabApi.Models.DTOs;

namespace BioLabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetalleController : ControllerBase
    {
        private readonly IDetalleService _detalleService;

        public DetalleController(IDetalleService detalleService)
        {
            _detalleService = detalleService;
        }

        // ==========================================
        // Metodos GET (Públicos)
        // ==========================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByOrdenId(int id)
        {
            var detalle = await _detalleService.GetDetallesByOrdenIdAsync(id);
            if (!detalle.Success)
            {
                return NotFound(detalle);
            }
            return Ok(detalle);
        }
        
        // ==========================================
        // Metodos POST y PUT (Operaciones críticas)
        // ==========================================

        // CORRECCIÓN: Se agregó el identificador de usuario en los Headers
        [HttpPost]
        public async Task<IActionResult> CreateDetalle([FromBody] DetalleCreateDTO detalle, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
        {
            var result = await _detalleService.CreateDetalleAsync(detalle, usuarioId); // Recuerda actualizar la interfaz IDetalleService en C#
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // CORRECCIÓN: Se cambió FromQuery por FromHeader y se estandarizó el nombre a usuarioId
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDetalle(int id, [FromBody] DetalleUpdateDTO detalle, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
        {
            var result = await _detalleService.UpdateDetalleAsync(detalle, usuarioId, id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}