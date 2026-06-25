using BioLabApi.Models;
using BioLabApi.Services.Interfaces;
using BioLabApi.Data;
using Microsoft.EntityFrameworkCore;
using BioLabApi.Helpers;
using BioLabApi.Models.DTOs;


namespace BioLabApi.Services.Servicios;

public class ExamenesService : IExamenesService
{   

    private readonly AppDbContext _appDbContext;
    
    public ExamenesService(AppDbContext dbContext)
    {
       _appDbContext = dbContext;
    }


    public async Task<ListOperationResult<ExamenResponseDTO>> GetExamenesAsync()
    {
        try
        {
            var examenes = await _appDbContext.Examenes
                .Select( e => new ExamenResponseDTO
                {
                    Id = e.Id,
                    NombreExamen = e.NombreExamen,
                    CostoEnDivisa = e.CostoEnDivisa,
                    Descripcion = e.Descripcion,
                    FechaCreacion = e.FechaCreacion,
                    CreadoPorId = e.CreadoPorId,
                    ModificadoPorId = e.ModificadoPorId,
                    FechaModificacion = e.FechaModificacion
                }).ToListAsync();
            
            return new ListOperationResult<ExamenResponseDTO>(true, "", Data: examenes);
        }
        catch (Exception ex) 
        {
            return new ListOperationResult<ExamenResponseDTO>(false, $"Error: \n{ex.Message}", Data: null);
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
            return new ObjectOperationResult(true, "", new ExamenResponseDTO
            {
                Id = examen.Id,
                NombreExamen = examen.NombreExamen,
                CostoEnDivisa = examen.CostoEnDivisa,
                Descripcion = examen.Descripcion,
                FechaCreacion = examen.FechaCreacion,
                CreadoPorId = examen.CreadoPorId,
                ModificadoPorId = examen.ModificadoPorId,
                FechaModificacion = examen.FechaModificacion
            });
        }
        catch (Exception ex)
        {
            return new ObjectOperationResult(false, $"Error: \n{ex.Message}", null);
        }
    }

    public async Task<OperationResult> CreateExamenAsync(ExamenCreateDTO examen, int AdminId)
    {
        try
        {
            var adminValidate = await _appDbContext.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == AdminId);


            var validationResult = ValidatePermisos(adminValidate);
            if (!validationResult.Success)
            {
                return new OperationResult(false, validationResult.Message);
            }
            

            _appDbContext.Examenes.Add(new ExamenModel
            {
                NombreExamen = examen.NombreExamen,
                CostoEnDivisa = examen.CostoEnDivisa,
                Descripcion = examen.Descripcion,
                FechaCreacion = DateTime.Now,
                CreadoPorId = AdminId,
                ModificadoPorId = AdminId,
                FechaModificacion = DateTime.Now
            });
            await _appDbContext.SaveChangesAsync();
            return new OperationResult(true, "Examen creado exitosamente.");
        }
        catch (Exception ex)
        {
            return new ObjectOperationResult(false, $"Error: \n{ex.Message}", null);
        }
    }

    public async Task<OperationResult> UpdateExamenAsync(ExamenUpdateDTO examen, int AdminId, int ExamenId)
    {   
        var existingExamen = await _appDbContext.Examenes.FirstOrDefaultAsync(e => e.Id == ExamenId);
        if (existingExamen == null)
        {
            return new OperationResult(false, "El examen al que intenta acceder no existe");
        }

        var Admin = await _appDbContext.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == AdminId);
        
        var validateAdmin = ValidatePermisos(Admin);
        if (!validateAdmin.Success) return validateAdmin;

        try
        {
            existingExamen.NombreExamen = examen.NombreExamen;
            existingExamen.CostoEnDivisa = examen.CostoEnDivisa;
            existingExamen.Descripcion = examen.Descripcion;
            existingExamen.ModificadoPorId = AdminId;
            existingExamen.FechaModificacion = DateTime.Now;

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
            .Include(u => u.Rol)
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


    //validar datos para actualizar Examen
    public OperationResult ValidatePermisos(UsuarioModel adminValidate)
    {
        if (adminValidate == null)
        {
            return new OperationResult(false, "Usuario administrador no encontrado.");
        }
        
        if (!adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.GestionarExamenes))
        {
            return new OperationResult(false, "El usuario no tiene permisos para gestionar exámenes.");
        }
        return new OperationResult(true, " ");
    }
    

}