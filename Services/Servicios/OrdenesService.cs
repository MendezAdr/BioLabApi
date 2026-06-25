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


    // metodos getters.
    public async Task<ObjectOperationResult> GetOrdenByIdAsync(int id, int AdminId)
    {
        try
        {   var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
            var validacion = ValidatePermisos(admin);
            if (!validacion.Success) return new ObjectOperationResult(false, validacion.Message, null);

            var orden = await _context.Ordenes
                .Include(o => o.Paciente)
                .Include(o => o.Pagos)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
                return new ObjectOperationResult(false, "La orden no existe.", null);

            return new ObjectOperationResult(true, " ", new OrdenResponseDTO
            {
                Id = orden.Id,
                PacienteId = orden.PacienteId,
                FechaOrden = orden.Fecha,
                Estado = orden.Estado,
                NumeroFactura = orden.NumeroFactura,
                TotalDivisa = orden.TotalDivisa,
                Detalles = new List<DetalleResponseDTO>(orden.Detalles.Select(d => new DetalleResponseDTO
                {
                    Id = d.Id,
                    ExamenId = d.ExamenId,
                    PrecioMomentoDivisa = d.PrecioMomentoDivisa
                })),
                Pagos = new List<PagoResponseDTO>(orden.Pagos.Select(p => new PagoResponseDTO
                {
                    Id = p.Id,
                    Metodo = p.Metodo,
                    Monto = p.Monto,
                    Referencia = p.Referencia
                }))

            });
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
            var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
            var validacion = ValidatePermisos(admin);
            if (!validacion.Success) return new ListOperationResult<OrdenResponseDTO>(false, validacion.Message, null);

            var lista = await _context.Ordenes
                .Include(o => o.Paciente)
                .OrderByDescending(o => o.Fecha)
                .AsNoTracking()
                .Select(o => new OrdenResponseDTO
                {
                    Id = o.Id,
                    PacienteId = o.PacienteId,
                    FechaOrden = o.Fecha,
                    Estado = o.Estado,
                    NumeroFactura = o.NumeroFactura,
                    TotalDivisa = o.TotalDivisa,
                    Detalles = new List<DetalleResponseDTO>(o.Detalles.Select(d => new DetalleResponseDTO
                    {
                        Id = d.Id,
                        ExamenId = d.ExamenId,
                        PrecioMomentoDivisa = d.PrecioMomentoDivisa
                    })),
                    Pagos = new List<PagoResponseDTO>(o.Pagos.Select(p => new PagoResponseDTO
                    {
                        Id = p.Id,
                        Metodo = p.Metodo,
                        Monto = p.Monto,
                        Referencia = p.Referencia
                    }))
                })
                .ToListAsync();

            return new ListOperationResult<OrdenResponseDTO>(true, " ", lista);
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
            var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
            var validacion = ValidatePermisos(admin);
            if (!validacion.Success) return new ListOperationResult<OrdenResponseDTO>(false, validacion.Message, null);

            var lista = await _context.Ordenes
                .Include(o => o.Paciente)
                .Where(o => o.Fecha.Date >= inicio.Date && o.Fecha.Date <= fin.Date)
                .Select(o => new OrdenResponseDTO
                {
                    Id = o.Id,
                    PacienteId = o.PacienteId,
                    FechaOrden = o.Fecha,
                    Estado = o.Estado,
                    NumeroFactura = o.NumeroFactura,
                    TotalDivisa = o.TotalDivisa,
                    Detalles = new List<DetalleResponseDTO>(o.Detalles.Select(d => new DetalleResponseDTO
                    {
                        Id = d.Id,
                        ExamenId = d.ExamenId,
                        PrecioMomentoDivisa = d.PrecioMomentoDivisa
                    })),
                    Pagos = new List<PagoResponseDTO>(o.Pagos.Select(p => new PagoResponseDTO
                    {
                        Id = p.Id,
                        Metodo = p.Metodo,
                        Monto = p.Monto,
                        Referencia = p.Referencia
                    }))
                })
                .AsNoTracking()
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
        var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
        var validacion = ValidatePermisos(admin);
        if (!validacion.Success) return new ListOperationResult<OrdenResponseDTO>(false, validacion.Message, null);

        var lista = await _context.Ordenes
            .Where(o => o.PacienteId == idPaciente)
            .Include(o => o.Paciente)
            .Select(o => new OrdenResponseDTO
            {
                Id = o.Id,
                PacienteId = o.PacienteId,
                FechaOrden = o.Fecha,
                Estado = o.Estado,
                NumeroFactura = o.NumeroFactura,
                TotalDivisa = o.TotalDivisa,
                Detalles = new List<DetalleResponseDTO>(o.Detalles.Select(d => new DetalleResponseDTO
                {
                    Id = d.Id,
                    ExamenId = d.ExamenId,
                    PrecioMomentoDivisa = d.PrecioMomentoDivisa
                })),
                Pagos = new List<PagoResponseDTO>(o.Pagos.Select(p => new PagoResponseDTO
                {
                    Id = p.Id,
                    Metodo = p.Metodo,
                    Monto = p.Monto,
                    Referencia = p.Referencia
                }))
            })
            .AsNoTracking()
            .ToListAsync();
        return new ListOperationResult<OrdenResponseDTO>(true, "", lista);
    }

    ///muy importante, verificar la transformacion de estado!!! y revisar el controlador tambien
    public async Task<ListOperationResult<OrdenResponseDTO>> GetAllOrdenesByEstadoAsync(OrdenesModel.EstadoPago estado, int AdminId)
    {
        var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
        var validacion = ValidatePermisos(admin);
        if (!validacion.Success) return new ListOperationResult<OrdenResponseDTO>(false, validacion.Message, null);

        var lista = await _context.Ordenes
            .Where(o => o.Estado == estado)
            .Include(o => o.Paciente)
            .Select(o => new OrdenResponseDTO
            {
                Id = o.Id,
                PacienteId = o.PacienteId,
                FechaOrden = o.Fecha,
                Estado = o.Estado,
                NumeroFactura = o.NumeroFactura,
                TotalDivisa = o.TotalDivisa,
                Detalles = new List<DetalleResponseDTO>(o.Detalles.Select(d => new DetalleResponseDTO
                {
                    Id = d.Id,
                    ExamenId = d.ExamenId,
                    PrecioMomentoDivisa = d.PrecioMomentoDivisa
                })),
                Pagos = new List<PagoResponseDTO>(o.Pagos.Select(p => new PagoResponseDTO
                {
                    Id = p.Id,
                    Metodo = p.Metodo,
                    Monto = p.Monto,
                    Referencia = p.Referencia
                }))
            })
            .AsNoTracking()
            .ToListAsync();
        return new ListOperationResult<OrdenResponseDTO>(true, "", lista);
    }

    
    // metodos de creacion, actualizacion y eliminacion.
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

                // Campos de auditoría
                CreadoPorId = UsuarioActualId,
                ModificadoPorId = UsuarioActualId
            };

            // 2. MAPEO DE DETALLES (Exámenes)
            foreach (var detalleDto in ordenDto.Detalles)
            {
                nuevaOrden.Detalles.Add(new DetalleModel
                {
                    ExamenId = detalleDto.ExamenId,
                    PrecioMomentoDivisa = detalleDto.PrecioMomentoDivisa

                });
            }

            // 3. MAPEO DE PAGOS Y CÁLCULO SIMULTÁNEO
            decimal totalPagadoNormalizado = 0;

            foreach (var pagoDto in ordenDto.Pagos)
            {
                // Agregamos el pago a la entidad real
                nuevaOrden.Pagos.Add(new PagosModel
                {
                    Metodo = pagoDto.Metodo,
                    Monto = pagoDto.Monto,
                    Referencia = pagoDto.Referencia ?? string.Empty // Evitar nulos

                });

                // Lógica Bimodal: Normalizar este pago específico a Divisa para el cálculo
                bool esPagoEnBs = (int)pagoDto.Metodo >= 1 && (int)pagoDto.Metodo <= 4;

                if (esPagoEnBs)
                    totalPagadoNormalizado += pagoDto.Monto / ordenDto.TasaBcv;
                else
                    totalPagadoNormalizado += pagoDto.Monto;
            }

            // 4. EL CÁLCULO DEL ESTADO (Tu lógica exacta)
            totalPagadoNormalizado = Math.Round(totalPagadoNormalizado, 2);
            var totalRequeridoDivisa = Math.Round(nuevaOrden.TotalDivisa, 2);

            if (totalPagadoNormalizado >= totalRequeridoDivisa)
            {
                nuevaOrden.Estado = OrdenesModel.EstadoPago.Pagado;
            }
            else if (totalPagadoNormalizado > 0)
            {
                nuevaOrden.Estado = OrdenesModel.EstadoPago.Parcial;
            }
            else
            {
                nuevaOrden.Estado = OrdenesModel.EstadoPago.Pendiente;
            }

            // 5. GUARDADO TRANSACCIONAL
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
            // 1. Cargar el registro original incluyendo sus grafos dependientes
            var ordenDb = await _context.Ordenes
                .Include(o => o.Detalles)
                .Include(o => o.Pagos)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordenDb == null) return new OperationResult(false, "La orden no existe.");

            // Validar permisos del Administrador (Garantiza RNF-6)
            var admin = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            var permisosValidacion = ValidatePermisos(admin);
            if (!permisosValidacion.Success) return permisosValidacion;

            // 2. BLINDAJE DE DATOS HISTÓRICOS (Regla del negocio)
            // Ignoramos PacienteId, TasaBcv y Fecha para mantener la consistencia fiscal intacta
            ordenDb.TotalDivisa = ordenDto.TotalDivisa; // Cambia si se añaden o remueven exámenes
            ordenDb.ModificadoPorId = usuarioId;
            ordenDb.FechaModificacion = DateTime.Now;

            // 3. SINCRONIZACIÓN DIFERENCIAL DE EXÁMENES (Detalles)
            // A. Remover exámenes que ya no vienen en el DTO
            var detallesEliminados = ordenDb.Detalles
                .Where(dDb => !ordenDto.Detalles.Any(dMod => dMod.Id == dDb.Id))
                .ToList();

            foreach (var detalleEliminado in detallesEliminados)
            {
                _context.Detalles.Remove(detalleEliminado);
            }

            // B. Agregar nuevos o actualizar montos de los existentes
            foreach (var dMod in ordenDto.Detalles)
            {
                if (dMod.Id == 0) // Es un examen nuevo agregado a la orden
                {
                    ordenDb.Detalles.Add(new DetalleModel
                    {
                        ExamenId = dMod.ExamenId,
                        PrecioMomentoDivisa = dMod.PrecioMomentoDivisa
                    });
                }
                else // Es un examen que ya estaba, verificamos consistencia
                {
                    var detalleExistente = ordenDb.Detalles.FirstOrDefault(d => d.Id == dMod.Id);
                    if (detalleExistente != null)
                    {
                        detalleExistente.ExamenId = dMod.ExamenId;
                        detalleExistente.PrecioMomentoDivisa = dMod.PrecioMomentoDivisa;
                    }
                }
            }

            // 4. SINCRONIZACIÓN DIFERENCIAL DE PAGOS
            // A. Remover pagos ausentes (Corrección contable)
            var pagosEliminados = ordenDb.Pagos
                .Where(pDb => !ordenDto.Pagos.Any(pMod => pMod.Id == pDb.Id))
                .ToList();

            foreach (var pagoEliminado in pagosEliminados)
            {
                _context.Pagos.Remove(pagoEliminado);
            }

            // B. Insertar abonos nuevos (Id == 0) o corregir transcripciones de existentes
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

            // 5. RECALCULAR EL ESTADO BIMODAL AUTOMÁTICAMENTE
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

            // 6. Persistencia física en SQLite
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


    /// muy importante, verificar la transformacion del estado!!!
    public async Task<OperationResult> UpdateEstadoOrdenAsync(int id, string nuevoEstado, int AdminId)
    {
        // 1. Validación de Permisos
        var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);

        var validacion = ValidatePermisos(admin);
        if (!validacion.Success) return new OperationResult(false, validacion.Message);

        // 2. Traducción de String a Enum (El corazón de la solución)
        // El parámetro 'true' hace que ignore mayúsculas y minúsculas (ej: "parcial" funcionará igual que "Parcial")
        if (!Enum.TryParse<OrdenesModel.EstadoPago>(nuevoEstado, true, out var estadoParseado))
        {
            return new OperationResult(false, $"El estado proporcionado ('{nuevoEstado}') no es válido en el sistema.");
        }

        // 3. Búsqueda de la Orden
        var orden = await _context.Ordenes.FindAsync(id);
        if (orden == null) return new OperationResult(false, "Orden no encontrada.");

        try
        {
            // 4. Asignación del tipo correcto
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
        // En lugar de borrar, podrías cambiar un estado a "Anulada"
        return await UpdateEstadoOrdenAsync(id, "Anulada", AdminId);
    }

    
    
    // Implementaciones adicionales de filtrado
    

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
