using BioLabApi.Models;
using BioLabApi.Services.Interfaces;
using BioLabApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using BioLabApi.Models.DTOs;


namespace BioLabApi.Controllers;

[ApiController] // Indica que esta clase es una API
[Route("api/[controller]")] // La ruta será: api/usuarios
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService; // Inyección de dependencia del servicio de usuarios

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    //=================================================
    //              metodos Post
    //=================================================
    // RF-1: Autenticación de Usuarios
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _usuarioService.LoginAsync(request.Username, request.Password);

        if (!result.Success)
            return Unauthorized(result); // Devuelve error 401

        return Ok(result); // Devuelve el objeto Usuario con su Rol (RF-2)
    }
        
    // RF-3: Registro de Usuarios
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UsuarioCreateDTO nuevoUsuario, [FromQuery] int adminId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _usuarioService.CreateUsuarioAsync(nuevoUsuario, adminId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    //=================================================
    //              metodos Get
    //=================================================
    // RF-3: Obtener lista para el Administrador
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int adminId)
    {
        var result = await _usuarioService.GetAllUsuariosAsync(adminId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] int adminId)
    {
        var result = await _usuarioService.GetUserByIdAsync(id, adminId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }



    //=================================================
    //              metodos Patch
    //=================================================

    // RF-3: Desactivar cuentas (Baja de usuarios)
    [HttpPatch("{id}/desactivar")]
    public async Task<IActionResult> Deactivate(int id, [FromQuery] int adminId)
    {
        var result = await _usuarioService.DeactivateUsuarioAsync(id, adminId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPatch("{id}/activar")]
    public async Task<IActionResult> Activate(int id, [FromQuery] int adminId)
    {
        var result = await _usuarioService.ActivateUsuarioAsync(id, adminId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    
    // RF-5: Restablecimiento de Credenciales
    [HttpPatch("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] string newPassword, [FromQuery] int adminId)
    {
        var result = await _usuarioService.ChangePasswordAsync(id, newPassword, adminId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // =================================================
    //              metodos Put
    // =================================================

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UsuarioUpdateDTO usuario, [FromQuery] int adminId)
    {   
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validación de seguridad: el ID de la URL debe coincidir con el del objeto
        if (id != usuario.Id)
            return BadRequest(new OperationResult(false, "El ID del usuario no coincide con la petición."));

        var result = await _usuarioService.UpdateUsuarioAsync(usuario, adminId);

        if (!result.Success)
        {
            // Si el mensaje indica que no existe, enviamos NotFound, de lo contrario BadRequest
            if (result.Message.Contains("no existe")) return NotFound(result);
            return BadRequest(result);
        }

        return Ok(result);
    }

}


// Clase auxiliar para el login
public record LoginRequest(string Username, string Password);