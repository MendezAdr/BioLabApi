using Microsoft.AspNetCore.Mvc;
using BioLabApi.Models;
using BioLabApi.Services.Interfaces;

namespace BioLabApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenesService _ordenesService;

    public OrdenesController(IOrdenesService ordenesService)
    {
        _ordenesService = ordenesService;
    }

    // RF-15: Procesar una nueva venta/orden completa
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrdenesModel nuevaOrden)
    {
        // El servicio debe encargarse de la transacción atómica
        var result = await _ordenesService.CreateOrdenAsync(nuevaOrden);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // RF-13: Obtener orden por ID con sus detalles y pagos
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] int adminId)
    {
        var result = await _ordenesService.GetOrdenByIdAsync(id, adminId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    // RF-13: Consultar histórico por rango de fechas
    [HttpGet("rango")]
    public async Task<IActionResult> GetByDateRange( [FromQuery] DateTime inicio, [FromQuery] DateTime fin, [FromQuery] int adminId)
    {
        var result = await _ordenesService.GetAllOrdenesEntreFechasAsync(inicio, fin, adminId);
        return Ok(result);
    }

    // RF-18: Anulación de ventas (Cambio de estado)
    [HttpPatch("{id}/anular")]
    public async Task<IActionResult> Anular(int id, [FromQuery] int adminId)
    {
        var result = await _ordenesService.DeactivateOrdenAsync(id, adminId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}