using BioLabApi.Models;
using BioLabApi.Data;
using BioLabApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BioLabApi.Helpers;
using BioLabApi.Models.DTOs;

namespace BioLabApi.Services.Servicios;

public class OrdenesService : IOrdenesService
{
    private readonly AppDbContext _context;

    public OrdenesService(AppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // MÉTODOS DE LECTURA (GETTERS)
    // ==========================================

    public async Task<ObjectOperationResult> GetOrdenByIdAsync(int id, int AdminId)
    {
        try
        {   
            var admin = await _context.Usuarios.Include(u => u.Rol).AsNoTracking().FirstOrDefaultAsync(u => u.Id == AdminId);
            var validacion = ValidatePermisos(admin);
            if (!validacion.Success) return new ObjectOperationResult(false, validacion.Message, null);

            var orden = await _context.Ordenes
                .Include(o => o.Paciente)
                .Include(o => o.Pagos)
                // CORRECCIÓN CRÍTICA: Incluimos explícitamente los detalles y cruzamos con Exámenes
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Examen) 
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
                return new ObjectOperationResult(false, "La orden no existe.", null);

            return new ObjectOperationResult(true, "Orden obtenida con éxito.", MapearOrdenADto(orden));
        }
        catch (Exception ex)
        {
            return new ObjectOperationResult(false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<ListOperationResult<OrdenResponseDTO>> GetAllOrdenesAsync(int AdminId)
    {
        try
        {
            var admin = await _context.Usuarios.Include(u => u.Rol).AsNoTracking().FirstOrDefaultAsync(u => u.Id == AdminId);
            var validacion = ValidatePermisos(admin);
            if (!validacion.Success) return new ListOperationResult<OrdenResponseDTO>(false, validacion.Message, null);

            // EF Core armará los JOINs automáticamente gracias a la proyección .Select()
            var lista = await _context.Ordenes
                .OrderByDescending(o => o.Fecha)
                .AsNoTracking()
                .Select(o => MapearOrdenADto(o))
                .ToListAsync();

            return new ListOperationResult<OrdenResponseDTO>(true, "Lista obtenida.", lista);
        }
        catch (Exception ex)
        {
            return new ListOperationResult<OrdenResponseDTO>(false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<ListOperationResult<OrdenResponseDTO>> GetAllOrdenesEntreFechasAsync(DateTime inicio, DateTime fin, int AdminId)
    {
        try
        {
            var admin = await _context.Usuarios.Include(u => u.Rol).AsNoTracking().FirstOrDefaultAsync(u => u.Id == AdminId);
            var validacion = ValidatePermisos(admin);
            if (!validacion.Success) return new ListOperationResult<OrdenResponseDTO>(false, validacion.Message, null);

            var lista = await _context.Ordenes
                .Where(o => o.Fecha.Date >= inicio.Date && o.Fecha.Date <= fin.Date)
                .AsNoTracking()
                .Select(o => MapearOrdenADto(o))
                .ToListAsync();

            return new ListOperationResult<OrdenResponseDTO>(true, "Búsqueda finalizada.", lista);
        }
        catch (Exception ex)
        {
            return new ListOperationResult<OrdenResponseDTO>(false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<ListOperationResult<OrdenResponseDTO>> GetAllOrdenesByPacienteAsync(int idPaciente, int AdminId)
    {
        var admin = await _context.Usuarios.Include(u => u.Rol).AsNoTracking().FirstOrDefaultAsync(u => u.Id == AdminId);
        var validacion = ValidatePermisos(admin);
        if (!validacion.Success) return new ListOperationResult<OrdenResponseDTO>(false, validacion.Message, null);

        var lista = await _context.Ordenes
            .Where(o => o.PacienteId == idPaciente)
            .AsNoTracking()
            .Select(o => MapearOrdenADto(o))
            .ToListAsync();
            
        return new ListOperationResult<OrdenResponseDTO>(true, "", lista);
    }

    public async Task<ListOperationResult<OrdenResponseDTO>> GetAllOrdenesByEstadoAsync(OrdenesModel.EstadoPago estado, int AdminId)
    {
        var admin = await _context.Usuarios.Include(u => u.Rol).AsNoTracking().FirstOrDefaultAsync(u => u.Id == AdminId);
        var validacion = ValidatePermisos(admin);
        if (!validacion.Success) return new ListOperationResult<OrdenResponseDTO>(false, validacion.Message, null);

        var lista = await _context.Ordenes
            .Where(o => o.Estado == estado)
            .AsNoTracking()
            .Select(o => MapearOrdenADto(o))
            .ToListAsync();
            
        return new ListOperationResult<OrdenResponseDTO>(true, "", lista);
    }

    // ==========================================
    // HELPER DE MAPEO CENTRALIZADO
    // ==========================================
    // Esto previene que olvides mapear un campo en el futuro.
    private static OrdenResponseDTO MapearOrdenADto(OrdenesModel orden)
    {
        return new OrdenResponseDTO
        {
            Id = orden.Id,
            PacienteId = orden.PacienteId,
            FechaOrden = orden.Fecha,
            Estado = orden.Estado,
            NumeroFactura = orden.NumeroFactura,
            TotalDivisa = orden.TotalDivisa,
            
            
            Detalles = orden.Detalles.Select(d => new DetalleResponseDTO
            {
                Id = d.Id,
                ExamenId = d.ExamenId,
                PrecioMomentoDivisa = d.PrecioMomentoDivisa,
                ExamenNombre = d.Examen != null ? d.Examen.NombreExamen : "Desconocido" // <- CORRECCIÓN: Nombre real
            }).ToList(),
            
            Pagos = orden.Pagos.Select(p => new PagoResponseDTO
            {
                Id = p.Id,
                Metodo = p.Metodo,
                Monto = p.Monto,
                Referencia = p.Referencia
            }).ToList()
        };
    }

    // ==========================================
    // MÉTODOS DE CREACIÓN Y ACTUALIZACIÓN (Se mantienen iguales)
    // ==========================================
    
    public async Task<OperationResult> CreateOrdenAsync(OrdenCreateDTO ordenDto, int UsuarioActualId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var nuevaOrden = new OrdenesModel
            {
                NumeroFactura = ordenDto.NumeroFactura,
                PacienteId = ordenDto.PacienteId,
                TotalDivisa = ordenDto.TotalDivisa,
                TasaBcv = ordenDto.TasaBcv,
                Fecha = ordenDto.Fecha == default ? DateTime.Now : ordenDto.Fecha,
                CreadoPorId = UsuarioActualId,
                ModificadoPorId = UsuarioActualId
            };

            foreach (var detalleDto in ordenDto.Detalles)
            {
                nuevaOrden.Detalles.Add(new DetalleModel
                {
                    ExamenId = detalleDto.ExamenId,
                    PrecioMomentoDivisa = detalleDto.PrecioMomentoDivisa
                });
            }

            decimal totalPagadoNormalizado = 0;

            foreach (var pagoDto in ordenDto.Pagos)
            {
                nuevaOrden.Pagos.Add(new PagosModel
                {
                    Metodo = pagoDto.Metodo,
                    Monto = pagoDto.Monto,
                    Referencia = pagoDto.Referencia ?? string.Empty 
                });

                bool esPagoEnBs = (int)pagoDto.Metodo >= 1 && (int)pagoDto.Metodo <= 4;
                if (esPagoEnBs)
                    totalPagadoNormalizado += pagoDto.Monto / ordenDto.TasaBcv;
                else
                    totalPagadoNormalizado += pagoDto.Monto;
            }

            totalPagadoNormalizado = Math.Round(totalPagadoNormalizado, 2);
            var totalRequeridoDivisa = Math.Round(nuevaOrden.TotalDivisa, 2);

            if (totalPagadoNormalizado >= totalRequeridoDivisa)
                nuevaOrden.Estado = OrdenesModel.EstadoPago.Pagado;
            else if (totalPagadoNormalizado > 0)
                nuevaOrden.Estado = OrdenesModel.EstadoPago.Parcial;
            else
                nuevaOrden.Estado = OrdenesModel.EstadoPago.Pendiente;

            await _context.Ordenes.AddAsync(nuevaOrden);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OperationResult(true, "Orden procesada correctamente.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OperationResult(false, $"Error crítico: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateOrdenAsync(int id, OrdenUpdateDTO ordenDto, int usuarioId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var ordenDb = await _context.Ordenes
                .Include(o => o.Detalles)
                .Include(o => o.Pagos)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordenDb == null) return new OperationResult(false, "La orden no existe.");

            var admin = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            var permisosValidacion = ValidatePermisos(admin);
            if (!permisosValidacion.Success) return permisosValidacion;

            ordenDb.TotalDivisa = ordenDto.TotalDivisa; 
            ordenDb.ModificadoPorId = usuarioId;
            ordenDb.FechaModificacion = DateTime.Now;

            var detallesEliminados = ordenDb.Detalles
                .Where(dDb => !ordenDto.Detalles.Any(dMod => dMod.Id == dDb.Id))
                .ToList();

            foreach (var detalleEliminado in detallesEliminados)
            {
                _context.Detalles.Remove(detalleEliminado);
            }

            foreach (var dMod in ordenDto.Detalles)
            {
                if (dMod.Id == 0) 
                {
                    ordenDb.Detalles.Add(new DetalleModel
                    {
                        ExamenId = dMod.ExamenId,
                        PrecioMomentoDivisa = dMod.PrecioMomentoDivisa
                    });
                }
                else 
                {
                    var detalleExistente = ordenDb.Detalles.FirstOrDefault(d => d.Id == dMod.Id);
                    if (detalleExistente != null)
                    {
                        detalleExistente.ExamenId = dMod.ExamenId;
                        detalleExistente.PrecioMomentoDivisa = dMod.PrecioMomentoDivisa;
                    }
                }
            }

            var pagosEliminados = ordenDb.Pagos
                .Where(pDb => !ordenDto.Pagos.Any(pMod => pMod.Id == pDb.Id))
                .ToList();

            foreach (var pagoEliminado in pagosEliminados)
            {
                _context.Pagos.Remove(pagoEliminado);
            }

            foreach (var pMod in ordenDto.Pagos)
            {
                if (pMod.Id == 0)
                {
                    ordenDb.Pagos.Add(new PagosModel
                    {
                        Monto = pMod.Monto,
                        Metodo = pMod.Metodo,
                        Referencia = pMod.Referencia ?? string.Empty
                    });
                }
                else
                {
                    var pagoExistente = ordenDb.Pagos.FirstOrDefault(p => p.Id == pMod.Id);
                    if (pagoExistente != null)
                    {
                        pagoExistente.Monto = pMod.Monto;
                        pagoExistente.Metodo = pMod.Metodo;
                        pagoExistente.Referencia = pMod.Referencia ?? string.Empty;
                    }
                }
            }

            decimal totalPagadoDivisa = 0;
            foreach (var p in ordenDb.Pagos)
            {
                bool esBs = (int)p.Metodo >= 1 && (int)p.Metodo <= 4;
                totalPagadoDivisa += esBs ? (p.Monto / ordenDb.TasaBcv) : p.Monto;
            }

            var totalRequerido = Math.Round(ordenDb.TotalDivisa, 2);
            totalPagadoDivisa = Math.Round(totalPagadoDivisa, 2);

            if (totalPagadoDivisa >= totalRequerido)
                ordenDb.Estado = OrdenesModel.EstadoPago.Pagado;
            else if (totalPagadoDivisa > 0)
                ordenDb.Estado = OrdenesModel.EstadoPago.Parcial;
            else
                ordenDb.Estado = OrdenesModel.EstadoPago.Pendiente;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OperationResult(true, "Orden sincronizada y corregida a través de DTOs con éxito.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OperationResult(false, $"Error crítico en la sincronización: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateEstadoOrdenAsync(int id, string nuevoEstado, int AdminId)
    {
        var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);

        var validacion = ValidatePermisos(admin);
        if (!validacion.Success) return new OperationResult(false, validacion.Message);

        if (!Enum.TryParse<OrdenesModel.EstadoPago>(nuevoEstado, true, out var estadoParseado))
        {
            return new OperationResult(false, $"El estado proporcionado ('{nuevoEstado}') no es válido en el sistema.");
        }

        var orden = await _context.Ordenes.FindAsync(id);
        if (orden == null) return new OperationResult(false, "Orden no encontrada.");

        try
        {
            orden.Estado = estadoParseado;
            orden.FechaModificacion = DateTime.Now;
            orden.ModificadoPorId = AdminId;

            await _context.SaveChangesAsync();
            return new OperationResult(true, $"Estado de la orden actualizado a {estadoParseado}.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error al actualizar la base de datos: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeactivateOrdenAsync(int id, int AdminId)
    {
        return await UpdateEstadoOrdenAsync(id, "Anulada", AdminId);
    }

    public OperationResult ValidatePermisos(UsuarioModel adminValidate)
    {
        if (adminValidate == null)
        {
            return new OperationResult(false, "Usuario administrador no encontrado.");
        }

        bool puedeOperarOrdenes = adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.CrearOrdenesYDetalles) ||
                              adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.Totalizar);

        if (!puedeOperarOrdenes)
        {
            return new OperationResult(false, "No tienes permisos para gestionar órdenes y ventas.");
        }

        return new OperationResult(true, " ");
    }
}