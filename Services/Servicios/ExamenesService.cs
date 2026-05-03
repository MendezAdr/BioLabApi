using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioLabProject.Models;
using BioLabProject.Services.Interfaces;
using BioLabProject.Data;
using Microsoft.EntityFrameworkCore;


namespace BioLabProject.Services.Servicios;

public class ExamenesService
{   

    private readonly AppDbContext _appDbContext;
    
    public ExamenesService(AppDbContext dbContext)
    {
       _appDbContext = dbContext;
    }


    public async Task<ListOperationResult<ExamenModel>> GetExamenesAsync()
    {
        try
        {
            return new ListOperationResult<ExamenModel>(true, "", Data: await _appDbContext.Examenes.ToListAsync());
        }
        catch (Exception ex) 
        {
            return new ListOperationResult<ExamenModel>(false, $"Error: \n{ex.Message}", Data: null);
        }
    }

    public async Task<ObjectOperationResult> GetExamenByIdAsync(int id) { 
        try
        {
            var examen = await _appDbContext.Examenes.FindAsync(id);
            if (examen == null)
            {
                return new ObjectOperationResult(false, "Examen no encontrado.", null);
            }
            return new ObjectOperationResult(true, "", examen);
        }
        catch (Exception ex)
        {
            return new ObjectOperationResult(false, $"Error: \n{ex.Message}", null);
        }
    }

    public async Task<ObjectOperationResult> CreateExamenAsync(ExamenModel examen, int AdminId)
    {
        try
        {
            var adminValidate = await _appDbContext.Usuarios.FindAsync(AdminId);
            var validationResult = ValidatePermisos(adminValidate);
            if (!validationResult.Success)
            {
                return new ObjectOperationResult(false, validationResult.Message, null);
            }
            var examenValidationResult = ValidateExamen(examen);
            if (!examenValidationResult.Success)
            {
                return new ObjectOperationResult(false, examenValidationResult.Message, null);
            }
            _appDbContext.Examenes.Add(examen);
            await _appDbContext.SaveChangesAsync();
            return new ObjectOperationResult(true, "Examen creado exitosamente.", examen);
        }
        catch (Exception ex)
        {
            return new ObjectOperationResult(false, $"Error: \n{ex.Message}", null);
        }
    }

    public async Task<OperationResult> UpdateExamenAsync(ExamenModel examen, int AdminId, int ExamenId)
    {   
        var existingExamen = await _appDbContext.Examenes.FirstOrDefaultAsync(e => e.Id == ExamenId);
        if (existingExamen == null)
        {
            return new OperationResult(false, "El examen al que intenta acceder no existe");
        }

        var validExamen = ValidateExamen(examen);
        if(!validExamen.Success) return validExamen;

        var Admin = await _appDbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == AdminId);
        
        var validateAdmin = ValidatePermisos(Admin);
        if (!validateAdmin.Success) return validateAdmin;

        try
        {
            existingExamen.NombreExamen = examen.NombreExamen;
            existingExamen.CostoEnDivisa = examen.CostoEnDivisa;
            existingExamen.CostoenBolivares = examen.CostoenBolivares;
            existingExamen.Descripcion = examen.Descripcion;

            await _appDbContext.SaveChangesAsync();
            return new OperationResult(true, "Examen actualizado exitosamente.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error: \n{ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteExamenAsync(int id, int AdminId)
    {
        var existingExamen = await _appDbContext.Examenes.FirstOrDefaultAsync(e => e.Id == id);
        if (existingExamen == null)
        {
            return new OperationResult(false, "El examen al que intenta acceder no existe");
        }
        var Admin = await _appDbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == AdminId);
        
        var validateAdmin = ValidatePermisos(Admin);
        if (!validateAdmin.Success) return validateAdmin;
        try
        {
            _appDbContext.Examenes.Remove(existingExamen);
            await _appDbContext.SaveChangesAsync();
            return new OperationResult(true, "Examen eliminado exitosamente.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error: \n{ex.Message}");
        }
    }



    //validaciones para crear examen
    public OperationResult ValidateExamen(ExamenModel examen)
    {
        if (examen.NombreExamen == null || examen.NombreExamen.Trim() == "")
        {
            return new OperationResult(false, "El nombre del examen es obligatorio.");
        }
        if (examen.CostoEnDivisa <= 0)
        {
            return new OperationResult(false, "El costo en divisa debe ser un número positivo.");
        }
        if (examen.CostoenBolivares <= 0)
        {
            return new OperationResult(false, "El costo en bolívares debe ser un número positivo.");
        }
        if (examen.Descripcion == null || examen.Descripcion.Trim() == "")
        {
            examen.Descripcion = "N/A";
        }
        return new OperationResult(true, "");

    }


    //validar datos para actualizar Examen
    public OperationResult ValidatePermisos(UsuarioModel adminValidate)
    {
        if (adminValidate == null)
        {
            return new OperationResult(false, "Usuario administrador no encontrado.");
        }
        
        if (!adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.ModificarExamenes))
        {
            return new OperationResult(false, "El usuario no tiene permisos para gestionar exámenes.");
        }
        return new OperationResult(true, " ");
    }
    

}