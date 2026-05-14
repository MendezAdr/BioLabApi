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

    public async Task<ListOperationResult<OrdenesModel>> GetAllOrdenesByEstadoAsync(string estado, int AdminId)
    {
        var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
        var validacion = ValidatePermisos(admin);
        if (!validacion.Success) return new ListOperationResult<OrdenesModel>(false, validacion.Message, null);

        var lista = await _context.Ordenes
            .Where(o => o.Estado.Equals(estado))
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

            // 2. Lógica Bimodal: Normalizar todos los pagos a Divisa
            decimal totalPagadoNormalizado = 0;

            foreach (var pago in orden)
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
            // 1. Cargar la orden actual con sus hijos (Detalles y Pagos)
            var ordenDb = await _context.Ordenes
                .Include(o => o.Detalles)
                .Include(o => o.Pagos)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordenDb == null) return new OperationResult(false, "La orden no existe.");

            var Admin = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);
            var permisosValidacion = ValidatePermisos(Admin);
            if (!permisosValidacion.Success) return new OperationResult(false, permisosValidacion.Message);

            // 2. Actualizar campos básicos de la orden
            ordenDb.TasaBcv = ordenModificada.TasaBcv;
            ordenDb.TotalDivisa = ordenModificada.TotalDivisa;
            ordenDb.ModificadoPorId = usuarioId; 
            ordenDb.FechaModificacion = DateTime.Now;

            // 3. Sincronizar Colecciones (Pagos/Detalles)
            // Nota: Para simplificar en tu proyecto, podrías limpiar y re-agregar, 
            // pero lo más profesional es actualizar los existentes.
            _context.Pagos.RemoveRange(ordenDb.Pagos);
            ordenDb.Pagos = ordenModificada.Pagos;

            _context.Detalles.RemoveRange(ordenDb.Detalles);
            ordenDb.Detalles = ordenModificada.Detalles;

            // 4. Recalcular el Estado Bimodal (Logica de Divisa vs Bolívares)
            decimal totalPagadoDivisa = 0;
            foreach (var pago in ordenDb.Pagos)
            {
                // Si el método es Bs (Punto, PagoMovil, etc. ), convertimos a divisa
                bool esBs = (int)pago.Metodo >= 1 && (int)pago.Metodo <= 4;
                totalPagadoDivisa += esBs ? (pago.Monto / ordenDb.TasaBcv) : pago.Monto;
            }

            // 5. Asignar nuevo estado automáticamente
            if (Math.Round(totalPagadoDivisa, 2) >= Math.Round(ordenDb.TotalDivisa, 2))
                ordenDb.Estado = OrdenesModel.EstadoPago.Pagado;
            else if (totalPagadoDivisa > 0 && totalPagadoDivisa < ordenDb.TotalDivisa )
                ordenDb.Estado = OrdenesModel.EstadoPago.Parcial;
            else
                ordenDb.Estado = OrdenesModel.EstadoPago.Pendiente;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OperationResult(true, "Orden y registros relacionados actualizados.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OperationResult(false, $"Error crítico: {ex.Message}");
        }
    }


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



    public OperationResult validatePagos(List<PagosModel> pagos)
    {
        foreach (var pago in pagos)
        {
        // metodo
        if (pago.Metodo == null) return new OperationResult(false, "No puedes registrar un pago sin especificar el metodo") break;
        // monto
        if (pago.Monto < 0 || decimal.IsNegative(pago.Monto) || pago.Monto == null) return new OperationResult(false, "Inserte un monto valido en el pago") break;
            // referencia
            if (pago.Metodo == PagosModel.MetodoPago.PagoMovil || PagosModel.MetodoPago.Transferencia)
            {
                if (string.IsNullOrEmpty(pago.Referencia)) return new OperationResult(false, "Inserte una referencia valida") break;
            }

        }
        return new OperationResult(true, "");
    }

    public OperationResult VerifyDetalles(List<DetalleModel> detalles)
    {
        foreach(var detalle in detalles)
        {
            if (detalle.Examen is null) return new OperationResult(false, "Error: debe haber al menos un examen para el detalle correspondiente") break; 
        } 

    }

   

}