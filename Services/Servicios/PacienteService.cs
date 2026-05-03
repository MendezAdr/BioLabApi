using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioLabProject.Models;
using BioLabProject.Services.Interfaces;
using BioLabProject.Data;
using Microsoft.EntityFrameworkCore;

namespace BioLabProject.Services.Servicios;

public class PacienteService : IPacientesService
{   
    private readonly AppDbContext _appDbContext;

    public PacienteService(AppDbContext dbContext)
    {
        _appDbContext = dbContext;
    }

    //retorna una lista de pacientes
    public async Task<ListOperationResult<PacienteModel>> GetAllPacientesAsync()
    {
        try
        {
            return new ListOperationResult<PacienteModel>(true, "Pacientes obtenidos correctamente.", await _appDbContext.Pacientes.ToListAsync());
        }
        catch (Exception ex)
        {
            return new ListOperationResult<PacienteModel>(false, $"Error al obtener pacientes: {ex.Message}", null);
        }
    }

    //retorna un paciente por su id
    public async Task<ObjectOperationResult> GetPacienteByIdAsync(int id)
    {
        try
        {
            var paciente = await _appDbContext.Pacientes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (paciente == null) return new ObjectOperationResult(false, "Paciente no encontrado.", null);
            

            return new ObjectOperationResult(true, "Paciente obtenido correctamente.", paciente);

        }
        catch (Exception ex)
        {
            return new ObjectOperationResult(false, $"Error al obtener paciente: {ex.Message}", null);
        }
    }
    
    //retorna un paciente por su nombre
    public async Task<ObjectOperationResult> GetByNombreAsync(string nombre)
    {
        try
        { 
            var paciente = await _appDbContext.Pacientes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Nombre == nombre);
            if (paciente == null) return new ObjectOperationResult(false, "Paciente no encontrado.", null);

            return new ObjectOperationResult(true, "Paciente obtenido correctamente.", paciente);
        }
        catch (Exception ex)
        {
            return new ObjectOperationResult(false, $"Error al obtener paciente: {ex.Message}", null);
        }
    }

    //retorna un paciente por su apellido
    public async Task<ObjectOperationResult> GetByApellidoAsync(string apellido)
    {
        try
        {
            var paciente = await _appDbContext.Pacientes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Apellido == apellido);
            if (paciente == null) return new ObjectOperationResult(false, "Paciente no encontrado.", null);

            return new ObjectOperationResult(true, "Paciente obtenido correctamente.", paciente);
        }
        catch (Exception ex)
        {
            return new ObjectOperationResult(false, $"Error al obtener paciente: {ex.Message}", null);
        }
    }

    //rerorna un paciente por su cedula
    public async Task<ObjectOperationResult> GetByCedulaAsync(string cedula)
    {
        try
        {
            var paciente = await _appDbContext.Pacientes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Cedula == cedula);
            if (paciente == null) return new ObjectOperationResult(false, "Paciente no encontrado.", null);
            return new ObjectOperationResult(true, "Paciente obtenido correctamente.", paciente);

        }
        catch (Exception ex)
        {
            return new ObjectOperationResult(false, $"Error al obtener paciente: {ex.Message}", null);

        }
    }


    //genericos

    //crea un paciente nuevo
    public async Task<OperationResult> CreateAsync(PacienteModel paciente)
    {
        var validationResult = ValidateInputUserData(paciente);

        if (!validationResult.Success)
        {
            return validationResult;
        }
        if (await _appDbContext.Pacientes.AnyAsync(p => p.Cedula == paciente.Cedula))
        {
            return new OperationResult(false, "Ya existe un paciente con la misma cédula.");
        }

        try
        {
            await _appDbContext.Pacientes.AddAsync(paciente);
            await _appDbContext.SaveChangesAsync();
            return new OperationResult(true, "Paciente creado correctamente.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error al crear paciente: {ex.Message}");
        }

    }

    //actualiza un paciente existente
    public async Task<OperationResult> UpdateAsync(PacienteModel paciente)
    {
        var validationResult = ValidateInputUserData(paciente);

        if (!validationResult.Success)
        {
            return validationResult;
        }
        if (await _appDbContext.Pacientes.AnyAsync(p => p.Id == paciente.Id) == false)
        {
            return new OperationResult(false, "Paciente no encontrado.");
        }
        try
        {   
            await _appDbContext.Pacientes.Where(p => p.Id == paciente.Id).ForEachAsync(p =>
            {
                p.Nombre = paciente.Nombre;
                p.Apellido = paciente.Apellido;
                p.Cedula = paciente.Cedula;
                p.Telefono = paciente.Telefono;
                p.Direccion = paciente.Direccion;
            });
            await _appDbContext.SaveChangesAsync();

            return new OperationResult(true, "Paciente actualizado correctamente.");
        }
        catch (Exception ex) 
        { 
            return new OperationResult(false, $"Error al actualizar paciente: {ex.Message}");
        }
    }

    //elimina (desactiva) un paciente por su id
    public async Task<OperationResult> DeactivateAsync(int id, int adminId) {
        
        var adminValidate = await _appDbContext.Usuarios.FindAsync(adminId);
        var permisosResult = ValidatePermisos(adminValidate);
        if (!permisosResult.Success)
        {
            return permisosResult;
        }
        if (await _appDbContext.Pacientes.AnyAsync(p => p.Id == id) == false)
        {
            return new OperationResult(false, "Paciente no encontrado.");
        }
        try
        {
            await _appDbContext.Pacientes.Where(p => p.Id == id).ForEachAsync(p =>
            {
                p.IsActive = false;
            });
            await _appDbContext.SaveChangesAsync();
            return new OperationResult(true, "Paciente desactivado correctamente.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error al desactivar paciente: {ex.Message}");
        }
    }

    //reactiva un paciente por su id
    public async Task<OperationResult> ActivateAsync(int id, int adminId)
    {
        var adminValidate = await _appDbContext.Usuarios.FindAsync(adminId);
        var permisosResult = ValidatePermisos(adminValidate);
        if (!permisosResult.Success)
        {
            return permisosResult;
        }
        if (await _appDbContext.Pacientes.AnyAsync(p => p.Id == id) == false)
        {
            return new OperationResult(false, "Paciente no encontrado.");
        }
        try
        {
            await _appDbContext.Pacientes.Where(p => p.Id == id).ForEachAsync(p =>
            {
                p.IsActive = true;
            });
            await _appDbContext.SaveChangesAsync();
            return new OperationResult(true, "Paciente reactivado correctamente.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error al reactivar paciente: {ex.Message}");
        }
    }


    public OperationResult ValidateInputUserData(PacienteModel paciente)
    {
        if (string.IsNullOrWhiteSpace(paciente.Nombre))
        {
            return new OperationResult(false, "El nombre del paciente no puede estar vacío.");
        }


        if (string.IsNullOrWhiteSpace(paciente.Apellido))
        {
            return new OperationResult(false, "El apellido del paciente no puede estar vacío.");
        }


        if (string.IsNullOrWhiteSpace(paciente.Cedula))
        {
            return new OperationResult(false, "La cédula del paciente no puede estar vacía.");
        }

        if (string.IsNullOrWhiteSpace(paciente.Telefono))
        {
            paciente.Telefono = "N/A";
        }

        if (string.IsNullOrWhiteSpace(paciente.Direccion))
        {
            paciente.Direccion = "N/A";
        }
        
        return new OperationResult(true, " ");

    }

    //validar datos para actualizar paciente
    public OperationResult ValidatePermisos(UsuarioModel adminValidate)
    {
        if (adminValidate == null)
        {
            return new OperationResult(false, "Usuario administrador no encontrado.");
        }

        if (!adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.ModificarPacientes))
        {
            return new OperationResult(false, "El usuario no tiene permisos para gestionar pacientes.");
        }

        return new OperationResult(true, " ");
    }


}