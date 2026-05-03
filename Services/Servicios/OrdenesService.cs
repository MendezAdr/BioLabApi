using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioLabProject.Models;
using BioLabProject.Data;
using BioLabProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BioLabProject.Services.Servicios;

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
            if (!validacion.Success) return new ObjectOperationResult(false, validacion.Message, null);

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
        try
        {
            // Validaciones básicas
            if (orden.PacienteId <= 0) return new OperationResult(false, "Debe asignar un paciente.");

            // La fecha se asigna al momento de crear si viene vacía
            if (orden.Fecha == default) orden.Fecha = DateTime.Now;

            await _context.Ordenes.AddAsync(orden);
            await _context.SaveChangesAsync();

            return new OperationResult(true, "Orden generada exitosamente.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error al guardar: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateEstadoOrdenAsync(int id, string nuevoEstado, int AdminId)
    {
        var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
        var validacion = ValidatePermisos(admin);
        if (!validacion.Success) return new ObjectOperationResult(false, validacion.Message, null);

        var orden = await _context.Ordenes.FindAsync(id);
        if (orden == null) return new OperationResult(false, "Orden no encontrada.");

        try
        {
            orden.Estado_Pago = nuevoEstado; // pagado, pendiente, parcial
            await _context.SaveChangesAsync();
            return new OperationResult(true, $"Estado actualizado a {nuevoEstado}.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error: {ex.Message}");
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
            if (!validacion.Success) return new ObjectOperationResult(false, validacion.Message, null);

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
    public async Task<ListOperationResult<OrdenesModel>> GetAllOrdenesPorPacienteAsync(int idPaciente, int AdminId)
    {
        var admin = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == AdminId);
        var validacion = ValidatePermisos(admin);
        if (!validacion.Success) return new ObjectOperationResult(false, validacion.Message, null);

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
        if (!validacion.Success) return new ObjectOperationResult(false, validacion.Message, null);

        var lista = await _context.Ordenes
            .Where(o => o.Estado_Pago == estado)
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