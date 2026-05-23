using Microsoft.AspNetCore.Mvc;
using BioLabApi.Services.Interfaces;
using BioLabApi.Models;


namespace BioLabAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]

    public class ExamenController : ControllerBase
    {
        private readonly IExamenesService _examenService;

        public ExamenController(IExamenesService examenService) => _examenService = examenService;


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _examenService.GetExamenesAsync();
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _examenService.GetExamenByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ExamenModel examen, [FromQuery] int AdminId)
        {
            var result = await _examenService.CreateExamenAsync(examen, AdminId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ExamenModel examen, [FromQuery] int AdminId)
        {
            var result = await _examenService.UpdateExamenAsync(examen, AdminId, id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int AdminId)
        {
            var result = await _examenService.DeleteExamenAsync(AdminId, id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

    }
}
