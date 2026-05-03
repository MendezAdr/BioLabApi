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

    public async Task<OperationResult> CreateOrdenAsync(OrdenesModel orden)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Validaciones iniciales
            if (orden.PacienteId <= 0)
                return new OperationResult(false, "Debe asignar un paciente.");

            if (orden.Fecha == default)
                orden.Fecha = DateTime.Now;

            if (orden.TasaBcv <= 0)
                return new OperationResult(false, "La tasa BCV del día debe ser mayor a cero.");

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

            await _context.SaveChangesAsync();
            return new OperationResult(true, $"Estado de la orden actualizado a {estadoParseado}.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error al actualizar la base de datos: {ex.Message}");
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

    public async Task<OperationResult> DeactivateOrdenAsync(int id, int AdminId)
    {
        // En lugar de borrar, podrías cambiar un estado a "Anulada"
        return await UpdateEstadoOrdenAsync(id, "Anulada", AdminId);
    }

    // Implementaciones adicionales de filtrado
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

    

    public OperationResult ValidatePermisos(UsuarioModel adminValidate)
    {
        if (adminValidate == null)
        {
            return new OperationResult(false, "Usuario administrador no encontrado.");
        }

        if (!adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.ModificarExamenes))
        {
            return new OperationResult(false, "El usuario no tiene permisos para modificar exámenes.");

        }

        return new OperationResult(true, " ");
    }

}