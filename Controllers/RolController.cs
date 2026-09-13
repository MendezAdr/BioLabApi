using BioLabApi.Models;
using BioLabApi.Models.DTOs;
using BioLabApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BioLabApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRolService _rolService;

        // Inyectamos la interfaz que creaste
        public RolesController(IRolService rolService)
        {
            _rolService = rolService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RolResponseDTO>>> GetAllRoles()
        {
            var roles = await _rolService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RolResponseDTO>> GetRolById(int id)
        {
            var rol = await _rolService.GetRolByIdAsync(id);
            
            if (rol == null)
            {
                return NotFound(new { message = $"No se encontró un rol con el ID {id}." });
            }

            return Ok(rol);
        }

        [HttpPost]
        public async Task<ActionResult<RolResponseDTO>> CreateRol([FromBody] RolCreateDTO rolCreateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var nuevoRol = await _rolService.CreateRolAsync(rolCreateDto);
                // Retornamos el DTO creado junto con la URL para acceder a él (código 201 Created)
                return CreatedAtAction(nameof(GetRolById), new { id = nuevoRol.Id }, nuevoRol);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RolResponseDTO>> UpdateRol(int id, [FromBody] RolUpdateDto rolUpdateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Verificamos que el ID de la ruta coincida con el ID del cuerpo[cite: 55]
            if (id != rolUpdateDto.Id)
            {
                return BadRequest(new { message = "El ID de la ruta no coincide con el ID del objeto a actualizar." });
            }

            try
            {
                var rolActualizado = await _rolService.UpdateRolAsync(rolUpdateDto);
                return Ok(rolActualizado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRol(int id)
        {
            try
            {
                var eliminado = await _rolService.DeleteRolAsync(id);
                
                if (!eliminado)
                {
                    return NotFound(new { message = $"No se encontró el rol con ID {id}." });
                }

                return Ok(new { message = "Rol eliminado correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                // Este catch atrapa el error si intentan borrar un rol que tiene usuarios asignados
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error interno del servidor.", details = ex.Message });
            }
        }
    }
}