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
    private readonly IUsuarioService _usuarioService; 
    private readonly GetDollarPrice _dollarPrice; 

    public UsuariosController(IUsuarioService usuarioService, GetDollarPrice dollarPrice)
    {
        _usuarioService = usuarioService;
        _dollarPrice = dollarPrice;
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
            return Unauthorized(result); 
        
        var response = new
        {
            Exito = true,
            Mensaje = result.Message,
            UsuarioInfo = result.objeto, 
            
            TasaDolar = _dollarPrice.IsSuccess ? _dollarPrice.CurrentRate?.Promedio : null,
            EstadoTasa = _dollarPrice.IsSuccess ? "Tasa del día obtenida" : "Fallo al obtener tasa, el front debe reintentar"
        };
        return Ok(response); 
    }
        
    // RF-3: Registro de Usuarios
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UsuarioCreateDTO nuevoUsuario, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _usuarioService.CreateUsuarioAsync(nuevoUsuario, usuarioId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    //=================================================
    //              metodos Get
    //=================================================
    
    // RF-3: Obtener lista para el Administrador
    [HttpGet]
    public async Task<IActionResult> GetAll([FromHeader(Name = "X-Usuario-Id")] int usuarioId)
    {
        var result = await _usuarioService.GetAllUsuariosAsync(usuarioId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
    {
        var result = await _usuarioService.GetUserByIdAsync(id, usuarioId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    //=================================================
    //              metodos Patch
    //=================================================

    // RF-3: Desactivar cuentas (Baja de usuarios)
    [HttpPatch("{id}/desactivar")]
    public async Task<IActionResult> Deactivate(int id, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
    {
        var result = await _usuarioService.DeactivateUsuarioAsync(id, usuarioId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPatch("{id}/activar")]
    public async Task<IActionResult> Activate(int id, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
    {
        var result = await _usuarioService.ActivateUsuarioAsync(id, usuarioId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    
    // RF-5: Restablecimiento de Credenciales
    [HttpPatch("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] string newPassword, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
    {
        var result = await _usuarioService.ChangePasswordAsync(id, newPassword, usuarioId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // =================================================
    //              metodos Put
    // =================================================

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UsuarioUpdateDTO usuario, [FromHeader(Name = "X-Usuario-Id")] int usuarioId)
    {   
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != usuario.Id)
            return BadRequest(new OperationResult(false, "El ID del usuario no coincide con la petición."));

        var result = await _usuarioService.UpdateUsuarioAsync(usuario, usuarioId);

        if (!result.Success)
        {
            if (result.Message.Contains("no existe")) return NotFound(result);
            return BadRequest(result);
        }

        return Ok(result);
    }
}

public record LoginRequest(string Username, string Password);