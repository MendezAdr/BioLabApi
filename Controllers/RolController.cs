using BioLabApi.Models.DTOs;
using BioLabApi.Services.Interfaces;
using BioLabApi.Helpers; // Asegura que este namespace apunte a donde están tus OperationResult
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BioLabApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRolService _rolService;

        public RolesController(IRolService rolService)
        {
            _rolService = rolService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _rolService.GetAllRolesAsync();
            
            if (!result.Success) return BadRequest(result);
            
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRolById(int id)
        {
            var result = await _rolService.GetRolByIdAsync(id);
            
            if (!result.Success) return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRol([FromBody] RolCreateDTO rolCreateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _rolService.CreateRolAsync(rolCreateDto);
            
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRol(int id, [FromBody] RolUpdateDto rolUpdateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Mantenemos la consistencia devolviendo un ObjectOperationResult si falla esta validación inicial
            if (id != rolUpdateDto.Id)
            {
                return BadRequest(new ObjectOperationResult(false, "El ID de la ruta no coincide con el ID del objeto a actualizar.", null));
            }

            var result = await _rolService.UpdateRolAsync(rolUpdateDto);
            
            if (!result.Success)
            {
                if (result.Message.Contains("no encontrado")) return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRol(int id)
        {
            var result = await _rolService.DeleteRolAsync(id);
            
            if (!result.Success)
            {
                if (result.Message.Contains("no encontrado")) return NotFound(result);
                // Retornamos BadRequest con el OperationResult intacto si falla por restricciones (ej. usuarios activos)
                return BadRequest(result); 
            }

            return Ok(result);
        }
    }
}