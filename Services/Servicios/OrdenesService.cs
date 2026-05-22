using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioLabApi.Models;
using BioLabApi.Data;
using BioLabApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BioLabApi.Helpers;

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

            return new ObjectOperationResult(true, " ", orden);
        }
        catch (Exception ex)
        {
            return new ObjectOperationResult(false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<ListOperationResult<OrdenesModel>> GetAllOrdenesAsync(int AdminId)
    {
        try
        {
            var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
            var validacion = ValidatePermisos(admin);
            if (!validacion.Success) return new ListOperationResult<OrdenesModel>(false, validacion.Message, null);

            var lista = await _context.Ordenes
                .Include(o => o.Paciente)
                .OrderByDescending(o => o.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return new ListOperationResult<OrdenesModel>(true, " ", lista);
        }
        catch (Exception ex)
        {
            return new ListOperationResult<OrdenesModel>(false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<ListOperationResult<OrdenesModel>> GetAllOrdenesEntreFechasAsync(DateTime inicio, DateTime fin, int AdminId)
    {
        try
        {
            var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
            var validacion = ValidatePermisos(admin);
            if (!validacion.Success) return new ListOperationResult<OrdenesModel>(false, validacion.Message, null);

            var lista = await _context.Ordenes
                .Include(o => o.Paciente)
                .Where(o => o.Fecha.Date >= inicio.Date && o.Fecha.Date <= fin.Date)
                .AsNoTracking()
                .ToListAsync();

            return new ListOperationResult<OrdenesModel>(true, "Búsqueda finalizada.", lista);
        }
        catch (Exception ex)
        {
            return new ListOperationResult<OrdenesModel>(false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<ListOperationResult<OrdenesModel>> GetAllOrdenesByPacienteAsync(int idPaciente, int AdminId)
    {
        var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
        var validacion = ValidatePermisos(admin);
        if (!validacion.Success) return new ListOperationResult<OrdenesModel>(false, validacion.Message, null);

        var lista = await _context.Ordenes
            .Where(o => o.PacienteId == idPaciente)
            .Include(o => o.Paciente)
            .AsNoTracking()
            .ToListAsync();
        return new ListOperationResult<OrdenesModel>(true, "", lista);
    }

    ///muy importante, verificar la transformacion de estado!!! y revisar el controlador tambien
    public async Task<ListOperationResult<OrdenesModel>> GetAllOrdenesByEstadoAsync(OrdenesModel.EstadoPago estado, int AdminId)
    {
        var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
        var validacion = ValidatePermisos(admin);
        if (!validacion.Success) return new ListOperationResult<OrdenesModel>(false, validacion.Message, null);

        var lista = await _context.Ordenes
            .Where(o => o.Estado == estado)
            .Include(o => o.Paciente)
            .AsNoTracking()
            .ToListAsync();
        return new ListOperationResult<OrdenesModel>(true, "", lista);
    }

    
    // metodos de creacion, actualizacion y eliminacion.
    public async Task<OperationResult> CreateOrdenAsync(OrdenesModel orden, int UsuarioActualId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            orden.CreadoPorId = UsuarioActualId; // Aseguramos que el ID del usuario esté asignado correctamente
            orden.ModificadoPorId = UsuarioActualId;
            orden.FechaModificacion = DateTime.Now;

            if (orden.Fecha == default) orden.Fecha = DateTime.Now;

          
            // 1. Validaciones iniciales
            if (orden.PacienteId <= 0)
                return new OperationResult(false, "Debe asignar un paciente.");

            if (orden.Fecha == default)
                orden.Fecha = DateTime.Now;

            if (orden.TasaBcv <= 0)
                return new OperationResult(false, "La tasa BCV del día debe ser mayor a cero.");
            
            var Details = VerifyDetalles(orden.Detalles);
            if (!Details.Success) return new OperationResult(false, Details.Message);
            
            var validPagos = ValidatePagos(orden.Pagos);
            if (!validPagos.Success) return new OperationResult(false, validPagos.Message);
            
            // 2. Lógica Bimodal: Normalizar todos los pagos a Divisa
            decimal totalPagadoNormalizado = 0;

            foreach (var pago in orden.Pagos)
            {
                
                bool esPagoEnBs = (int)pago.Metodo >= 1 && (int)pago.Metodo <= 4;

                if (esPagoEnBs)
                {
                    // Convertimos lo que pagó en Bs a Divisas usando la tasa del día de la orden
                    totalPagadoNormalizado += pago.Monto / orden.TasaBcv;
                }
                else
                {
                    // Si ya es divisa, se suma directamente
                    totalPagadoNormalizado += pago.Monto;
                }
            }

            // 3. Evitar problemas de precisión decimal (redondeamos a 2 decimales)
            totalPagadoNormalizado = Math.Round(totalPagadoNormalizado, 2);
            var totalRequeridoDivisa = Math.Round(orden.TotalDivisa, 2);

            // 4. Asignar el Estado de Pago dinámicamente
            if (totalPagadoNormalizado >= totalRequeridoDivisa)
            {
                orden.Estado = OrdenesModel.EstadoPago.Pagado;
            }
            else if (totalPagadoNormalizado > 0)
            {
                orden.Estado = OrdenesModel.EstadoPago.Parcial;
            }
            else
            {
                orden.Estado = OrdenesModel.EstadoPago.Pendiente;
            }

            // 5. Guardado transaccional
            await _context.Ordenes.AddAsync(orden);
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

    public async Task<OperationResult> UpdateOrdenAsync(int id, OrdenesModel ordenModificada, int usuarioId)
    {
    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        // 1. Cargar el registro original de la base de datos con sus listas hijas
        var ordenDb = await _context.Ordenes
            .Include(o => o.Detalles)
            .Include(o => o.Pagos)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (ordenDb == null) return new OperationResult(false, "La orden no existe.");

        // Validación estricta de permisos del Administrador
        var admin = await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);
        var permisosValidacion = ValidatePermisos(admin);
        if (!permisosValidacion.Success) return new OperationResult(false, permisosValidacion.Message);

        // Validamos el estado de los datos que vienen del frontend antes de operar
        var detailsValid = VerifyDetalles(ordenModificada.Detalles);
        if (!detailsValid.Success) return new OperationResult(false, detailsValid.Message);
        
        var validPagos = ValidatePagos(ordenModificada.Pagos);
        if (!validPagos.Success) return new OperationResult(false, validPagos.Message);

        // 2. ACTUALIZACIÓN SELECTIVA (Blindaje de datos maestros)
        // Ignoramos PacienteId, TasaBcv y Fecha para mantener el registro histórico intacto
        ordenDb.TotalDivisa = ordenModificada.TotalDivisa;
        ordenDb.ModificadoPorId = usuarioId; 
        ordenDb.FechaModificacion = DateTime.Now;

        // 3. SINCRONIZACIÓN DIFERENCIAL DE PAGOS (Sin RemoveRange)
        // A. Detectar y eliminar pagos que ya no vienen en la petición (si aplica la lógica)
        var pagosEliminados = ordenDb.Pagos
            .Where(pDb => !ordenModificada.Pagos.Any(pMod => pMod.Id == pDb.Id))
            .ToList();
            
        foreach (var pagoEliminado in pagosEliminados)
        {
            _context.Pagos.Remove(pagoEliminado);
        }

        // B. Actualizar existentes o agregar nuevos abonos
        foreach (var pagoModificado in ordenModificada.Pagos)
        {
            if (pagoModificado.Id == 0)
            {
                // Es un nuevo abono para completar una orden parcial/pendiente
                pagoModificado.OrdenId = ordenDb.Id;
                ordenDb.Pagos.Add(pagoModificado);
            }
            else
            {
                // Es una corrección de un pago existente (monto, referencia, método)
                var pagoExistente = ordenDb.Pagos.FirstOrDefault(p => p.Id == pagoModificado.Id);
                if (pagoExistente != null)
                {
                    pagoExistente.Monto = pagoModificado.Monto;
                    pagoExistente.Referencia = pagoModificado.Referencia;
                    pagoExistente.Metodo = pagoModificado.Metodo;
                    // El OrdenId y el Id de este pago no se alteran, protegiendo la auditoría
                }
            }
        }

        // 4. SINCRONIZACIÓN DIFERENCIAL DE DETALLES (EXÁMENES VENDIDOS)
        var detallesEliminados = ordenDb.Detalles
            .Where(dDb => !ordenModificada.Detalles.Any(dMod => dMod.Id == dDb.Id))
            .ToList();

        foreach (var detalleEliminado in detallesEliminados)
        {
            _context.Detalles.Remove(detalleEliminado);
        }

        foreach (var detalleModificado in ordenModificada.Detalles)
        {
            if (detalleModificado.Id == 0)
            {
                detalleModificado.OrdenId = ordenDb.Id;
                ordenDb.Detalles.Add(detalleModificado);
            }
            else
            {
                var detalleExistente = ordenDb.Detalles.FirstOrDefault(d => d.Id == detalleModificado.Id);
                if (detalleExistente != null)
                {
                    detalleExistente.ExamenId = detalleModificado.ExamenId;
                    detalleExistente.PrecioMomentoDivisa = detalleModificado.PrecioMomentoDivisa;
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

        // 6. Confirmación de Persistencia
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new OperationResult(true, "Orden y registros asociados corregidos mediante actualización diferencial.");
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

        bool puedeOperarOrdenes = adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.CrearVenta) ||
                              adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.HacerCierre);

        if (!puedeOperarOrdenes)
        {
            return new OperationResult(false, "No tienes permisos para gestionar órdenes y ventas.");
        }

        return new OperationResult(true, " ");
    }


    public OperationResult ValidatePagos(List<PagosModel> pagos)
    {
        foreach (var pago in pagos)
        {
            // No es necesario validar "null" para un enum, pero sí que sea mayor a 0
            if ((int)pago.Metodo <= 0)
                return new OperationResult(false, "No puedes registrar un pago sin especificar el metodo válido.");

            // Validamos el monto
            if (pago.Monto <= 0)
                return new OperationResult(false, "Inserte un monto válido mayor a cero en el pago.");

            // Corregido: Sin el punto y coma traicionero y simplificado
            bool esPagoDigital = pago.Metodo == PagosModel.MetodoPago.PagoMovil || 
                                 pago.Metodo == PagosModel.MetodoPago.Transferencia;

            if (esPagoDigital && string.IsNullOrWhiteSpace(pago.Referencia))
            {
                return new OperationResult(false, "Los pagos digitales requieren una referencia obligatoria.");
            }
        }
        return new OperationResult(true, "");
    }

    
    public OperationResult VerifyDetalles(List<DetalleModel> detalles)
    {
        if (detalles == null || detalles.Count == 0)
            return new OperationResult(false, "La orden debe contener al menos un detalle (examen).");

        foreach(var detalle in detalles)
        {
            // Validamos el ID, ya que el objeto Examen completo podría no venir del frontend
            if (detalle.ExamenId <= 0) 
                return new OperationResult(false, "Error: debe haber al menos un examen válido para cada detalle.");
        
            if (detalle.PrecioMomentoDivisa < 0)
                return new OperationResult(false, "El precio del examen no puede ser negativo.");
        } 
        return new OperationResult(true, "");
    }

   

}
