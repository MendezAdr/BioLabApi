using BioLabApi.Services.Interfaces;
using BioLabApi.Models;
using Microsoft.AspNetCore.Mvc;
using BioLabApi.Models.DTOs;

namespace BioLabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacienteController : ControllerBase
    {
        private readonly IPacientesService _pacienteService;
        public PacienteController(IPacientesService pacientesService) 
        {
            _pacienteService = pacientesService;
        }

        // ==========================================
        // Metodos GET (Públicos / Sin auditoría estricta)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Get() {
            var result = await _pacienteService.GetAllPacientesAsync();
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _pacienteService.GetPacienteByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("buscar/nombre/{nombre}")]
        public async Task<IActionResult> GetByNombre(string nombre)
        {
            var result = await _pacienteService.GetByNombreAsync(nombre);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("buscar/apellido/{apellido}")]
        public async Task<IActionResult> GetByApellido(string apellido)
        {
            var result = await _pacienteService.GetByApellidoAsync(apellido);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("buscar/cedula/{cedula}")]
        public async Task<IActionResult> GetByCedula(string cedula)
        {
            var result = await _pacienteService.GetByCedulaAsync(cedula);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }


        // ==========================================
        // Metodos POST y PATCH (Operaciones críticas)
        // ==========================================

        // CORRECCIÓN: Usar Cabecera
        [HttpPost()]
        public async Task<IActionResult> Create([FromBody] PacienteCreateDTO paciente, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
        {
            var result = await _pacienteService.CreateAsync(paciente, usuarioId);
            if (!result.Success) { return BadRequest(result); }
            return Ok(result);
        }

        // CORRECCIÓN: Agregado el identificador de usuario para auditoría
        [HttpPatch("{id}/actualizar")]
        public async Task<IActionResult> Update([FromBody] PacienteUpdateDTO paciente, [FromRoute] int id, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
        {
            var result = await _pacienteService.UpdateAsync(paciente, id, usuarioId); // Asegúrate de actualizar tu IPacientesService
            if(!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // CORRECCIÓN: Usar Cabecera
        [HttpPatch("{id}/Desactivar")]
        public async Task<IActionResult> Deactivate(int id, [FromHeader(Name = "X-Usuario-Id")] int usuarioId )
        {
            var result = await _pacienteService.DeactivateAsync(id, usuarioId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // CORRECCIÓN: Usar Cabecera. (El boolean State se queda por Query)
        [HttpPatch("{id}/activar")]
        public async Task<IActionResult> Activate(int id, [FromHeader(Name = "X-Usuario-Id")] int usuarioId, [FromQuery] bool State)
        {
            var result = await _pacienteService.ActivateAsync(id, usuarioId, State);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}