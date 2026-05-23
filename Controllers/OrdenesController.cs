using Microsoft.AspNetCore.Mvc;
using BioLabApi.Models;
using BioLabApi.Services.Interfaces;

namespace BioLabApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenesService _ordenesService;

    public OrdenesController(IOrdenesService ordenesService) => _ordenesService = ordenesService;

    [HttpGet]
    public async Task<IActionResult> Get([FromHeader(Name = "X-Admin-Id")] int adminId)
    {
        var result = await _ordenesService.GetAllOrdenesAsync(adminId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    // RF-13: Obtener orden por ID con sus detalles y pagos
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromHeader(Name = "X-Admin-Id")] int adminId)
    {
        var result = await _ordenesService.GetOrdenByIdAsync(id, adminId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    // RF-13: Consultar histórico por rango de fechas
    [HttpGet("rango")]
    public async Task<IActionResult> GetByFechas([FromQuery] DateTime inicio, [FromQuery] DateTime fin, [FromHeader(Name = "X-Admin-Id")] int adminId)
    {
        var result = await _ordenesService.GetAllOrdenesEntreFechasAsync(inicio, fin, adminId);
        return Ok(result);
    }

    [HttpGet("paciente/{pacienteId}")]
    public async Task<IActionResult> GetByPaciente([FromRoute] int pacienteId, [FromHeader(Name = "X-Admin-Id")] int adminId)
    {
        var result = await _ordenesService.GetAllOrdenesByPacienteAsync(pacienteId, adminId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("estado/{estado}")]
    public async Task<IActionResult> GetByEstado([FromRoute] OrdenesModel.EstadoPago estado, [FromHeader(Name = "X-Admin-Id")] int adminId)
    {
        var result = await _ordenesService.GetAllOrdenesByEstadoAsync(estado, adminId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    // RF-15: Procesar una nueva venta/orden completa
    [HttpPost]
    public async Task<IActionResult> Create([FromHeader(Name = "X-Usuario-Id")] int usuarioId, [FromBody] OrdenesModel nuevaOrden)
    {
        var result = await _ordenesService.CreateOrdenAsync(nuevaOrden, usuarioId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // RF-18: Anulación de ventas (Cambio de estado)
    [HttpPatch("{id}/anular")]
    public async Task<IActionResult> Anular([FromRoute] int id, [FromHeader(Name = "X-Admin-Id")] int adminId)
    {
        var result = await _ordenesService.DeactivateOrdenAsync(id, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id}/actualizar")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] OrdenesModel ordenActualizada, [FromHeader(Name = "X-Admin-Id")] int adminId)
    {
        var result = await _ordenesService.UpdateOrdenAsync(id, ordenActualizada, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }



}