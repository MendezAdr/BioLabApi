using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using BioLabApi.Models;
using BioLabApi.Services;
using BioLabApi.Data;
using BioLabApi.Helpers;
using Microsoft.Extensions.DependencyInjection;
using BioLabApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BioLabApi.Services.Servicios;


public class DetalleService : IDetalleService
{
private readonly AppDbContext _context;

    public DetalleService(AppDbContext context) 
    { 
        _context = context;   
    }

    // metodos generales

    // obtener el detalle por ID
    public async Task<ObjectOperationResult> GetDetalleByIdAsync(int id)
    {
        try
        {
            var Detalle = await _context.Detalles.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (Detalle == null) return new ObjectOperationResult(false, "El detalle asociado al Id no existe", null);
            return new ObjectOperationResult(true, "", Detalle);
                    
        }
        catch (Exception ex)
        {
            return new ObjectOperationResult(false, $"Error: {ex.Message}", null);
        }

    }

    // Obtener una lista de detalles por el id de la orden
    public async Task<ListOperationResult<DetalleModel>> GetDetallesByOrdenIdAsync(int oid)
    {
        try
        {
            var DetalleList =await _context.Detalles
                .AsNoTracking()
                .Where(e => e.OrdenId == oid)
                .ToListAsync();


            if (DetalleList == null) return new ListOperationResult<DetalleModel>(false, "No existe ningun detalle asociado a esa orden", null);

            return new ListOperationResult<DetalleModel>(true, "", DetalleList);

        }
        catch (Exception ex) 
        {
            return new ListOperationResult<DetalleModel>(false, $"Error: {ex.Message}", null);

        }

    }

    // obtener un detalle por el id del examen relacionado
    public async Task<ObjectOperationResult> GetDetalleByExamenIdAsync(int Eid)
    {
        try
        {
            var DetalleList = await _context.Detalles
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.ExamenId == Eid);


            if (DetalleList == null) return new ListOperationResult<DetalleModel>(false, "No existe ningun detalle asociado a ese Examen", null);

            return new ListOperationResult<DetalleModel>(true, "", DetalleList);

        }
        catch (Exception ex)
        {
            return new ListOperationResult<DetalleModel>(false, $"Error: {ex.Message}", null);

        } 
    }
    
    // crear un detalle
    public async Task<OperationResult> CreateDetalleAsync(DetalleModel detalle)
    {
        var DetalleExists = await _context.Detalles.AnyAsync(o => o.Id == detalle.Id);
        if (DetalleExists) return new OperationResult(false, "Ya existe un detalle con ese Id");

        try
        {
            _context.Detalles.Add(detalle);
            await _context.SaveChangesAsync();
            return new OperationResult(true, "Detalle creado exitosamente");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error: {ex.Message}");
        }

    }

    // actualiza un detalle
    public async Task<OperationResult> UpdateDetalleAsync(DetalleModel detalle, int AdminId)
    {   
        var DetalleExists = await _context.Detalles.AnyAsync(o => o.Id == detalle.Id);

        if (!DetalleExists) return new OperationResult(false, "No existe un detalle con ese Id");
        var adminValidate = await _context.Usuarios
            .Include(u => u.Rol)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == AdminId);
        var permisosCheck = ValidatePermisos(adminValidate);
        if (!permisosCheck.Success) return permisosCheck;
        try
        {
            _context.Detalles.Update(detalle);
            await _context.SaveChangesAsync();
            return new OperationResult(true, "Detalle actualizado exitosamente");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error: {ex.Message}");
        }


    }

    //metodos auxiliares.

    public OperationResult ValidateDetalle(DetalleModel detalle)
    {
        if (detalle == null) return new OperationResult(false, "El detalle no puede ser nulo");
        if (detalle.OrdenId <= 0) return new OperationResult(false, "El Id de la orden debe ser mayor a 0");
        if (detalle.ExamenId <= 0) return new OperationResult(false, "El Id del examen debe ser mayor a 0");
        if (detalle.Precio < 0) return new OperationResult(false, "El precio no puede ser negativo");
        return new OperationResult(true, "");
    }

    public OperationResult ValidatePermisos(UsuarioModel adminValidate)
    {
        if (adminValidate == null)
        {
            return new OperationResult(false, "Usuario administrador no encontrado.");
        }
        // Importante acomodar todos los permisos.
        if (!adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.ModificarPacientes))
        {
            return new OperationResult(false, "El usuario no tiene permisos para modificar detalles.");

        }

        return new OperationResult(true, " ");
    }

}