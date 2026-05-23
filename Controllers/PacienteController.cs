using BioLabApi.Services.Interfaces;
using BioLabApi.Models;
using Microsoft.AspNetCore.Mvc;


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

        //metodos GET

        [HttpGet]
        public async Task<IActionResult> Get() {

            var result = await _pacienteService.GetAllPacientesAsync();

            if (!result.Success) return NotFound(result);

            return Ok(result);

        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAll(int Id)
        {
            var result = await _pacienteService.GetPacienteByIdAsync(Id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("buscar/nombre/{nombre}")]
        public async Task<IActionResult> GetByNombre( string nombre)
        {
            var result = await _pacienteService.GetByNombreAsync(nombre);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("buscar/apellido/{apellido}")]
        public async Task<IActionResult> GetByApellido( string apellido)
        {
            var result = await _pacienteService.GetByApellidoAsync(apellido);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("buscar/cedula/{cedula}")]
        public async Task<IActionResult> GetByCedula( string cedula)
        {
            var result = await _pacienteService.GetByCedulaAsync(cedula);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }


        // crear un paciente
        [HttpPost()]
        public async Task<IActionResult> Create([FromBody] PacienteModel paciente, [FromQuery] int UserId)
        {
            var result = await _pacienteService.CreateAsync(paciente, UserId);
            if (!result.Success) { return BadRequest(result); }
            return Ok(result);

        }

        //actualizar un paciente
        [HttpPatch("{id:int}/actualizar")]
        public async Task<IActionResult> Update([FromBody] PacienteModel paciente, [FromRoute] int id)
        {
            var result = await _pacienteService.UpdateAsync(paciente, id);
            if(!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("{id:int}/Desactivar")]
        public async Task<IActionResult> Deactivate(int id, [FromQuery] int adminID )
        {
            var result = await _pacienteService.DeactivateAsync(id, adminID);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("{id:int}/activar")]
        public async Task<IActionResult> Activate(int id, [FromQuery] int adminID, [FromQuery] bool State)
        {
            var result = await _pacienteService.ActivateAsync(id, adminID, State);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }






    }
}
