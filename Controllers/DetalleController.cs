using Microsoft.AspNetCore.Mvc;
using BioLabApi.Models;
using BioLabApi.Services.Interfaces;


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
        [HttpPost]
        public async Task<IActionResult> CreateDetalle([FromBody] DetalleModel detalle)
        {
            var result = await _detalleService.CreateDetalleAsync(detalle);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDetalle(int id, [FromBody]DetalleModel detalle, [FromQuery] int AdminId)
        {
            var result = await _detalleService.UpdateDetalleAsync(detalle, AdminId, id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}
