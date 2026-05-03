using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using BioLabProject.Models;
using BioLabProject.Services;
using BioLabProject.Data;
using BioLabProject.Helpers;
using Microsoft.Extensions.DependencyInjection;
using BioLabProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BioLabProject.Services.Servicios;

public class UsuarioService : IUsuarioService
{
    /*
     * Controlador General de Usuarios
     * se supone que es para el login.
     */
    private readonly AppDbContext _context;
    // Inyeccion de Dependencias:

    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }
    
    
    // Loguearse
    public async Task<ObjectOperationResult?> LoginAsync(string username, string password)
    {
        var UserExist = await _context.Usuarios
            .Include(u => u.Rol)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Username == username);
        if (UserExist == null) return new ObjectOperationResult(false, "El Nombre de Usuario no existe, verifique e intente nuevamente.", null);

        var ValidPass = VerifyPassword(UserExist.Contrasena, password);
        if (!ValidPass) return new ObjectOperationResult(false, "Contrasena incorrecta, intente nuevamente", null);

        return new ObjectOperationResult(true, "Bienvenido", UserExist);
        //el resto de la logica de esto no corresponde a un servicio.
    }
    
    // desloguearse. No se si esto realmente sea util. 
    // deberia aplicarlo en una capa superior, tal vez como en ViewModel
    public Task<OperationResult?> LogOutAsync()
    {
        throw new NotImplementedException();
    }
    
    // contrasena

    public async Task<OperationResult> ChangePasswordAsync(int usuarioId, string newPassword, int adminId)
    { //importante, buscar el metodo para hashear la contrasena
        var adminValidate = await _context.Usuarios
            .Include(u => u.Rol)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == adminId);

        var validatePermisos = ValidatePermisos(adminValidate);
        if (!validatePermisos.Success) return new OperationResult(false, validatePermisos.Message);

        var userExistence = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == adminId);

        if (userExistence == null) return new OperationResult(false, "El usuario no Existe");

        try
        {
            userExistence.Id = userExistence.Id;
            userExistence.Username = userExistence.Username;
            userExistence.Nombre = userExistence.Nombre;
            userExistence.Apellido = userExistence.Apellido;
            userExistence.Cedula = userExistence.Cedula;
            userExistence.IsActive = userExistence.IsActive;
            userExistence.RolId = userExistence.RolId;
            userExistence.Rol = userExistence.Rol;

                    //solo se actualiza la contrasena se hashea automaticamente en el metodo HashPassword
                    userExistence.Contrasena = HashPassword(newPassword);
                
            await _context.SaveChangesAsync();
            return new OperationResult(true, "Contrasena actualizada exitosamente");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error: \n{ex.Message}");
        }
        
    }

    // obtener usuario especifico

    public async Task<ObjectOperationResult> GetUserByIdAsync(int id, int adminId)
    {
        var adminValidate = await _context.Usuarios
            .Include(u => u.Rol)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == adminId);
		
        var validatePermisos = ValidatePermisos(adminValidate);
        if (!validatePermisos.Success) return new ObjectOperationResult(false, validatePermisos.Message, null);

        try
        {	
            var User = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (User == null) return new ObjectOperationResult(false, "El usuario solicitado no existe.", null);
			
            return new ObjectOperationResult(true, "",  User);
        }
        catch (Exception e)
        {
            return new ObjectOperationResult(false, $"Error {e.Message}", null);
        }
    }

    // crear usuario
    public async Task<OperationResult?> CreateUsuarioAsync(UsuarioModel usuario, int AdminId)
    {
        var validationResult = ValidateInputUserData(usuario);
        if (!validationResult.Success) return validationResult;

        bool validateExistence = await _context.Usuarios.AnyAsync(u => u.Username == usuario.Username);
        if (validateExistence) return new OperationResult ( false, "El nombre de usuario ya existe. Por favor, elige otro." );

        try
        {
            _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
            return new OperationResult(true, "Usuario creado con éxito.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error de base de datos: {ex.Message}");
        }
    }
    
    // actualizar usuario
    public async Task<OperationResult?> UpdateUsuarioAsync(UsuarioModel usuario, int adminId)
    {
        var adminValidate = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == adminId);
		
        bool validateExistence = await _context.Usuarios.AnyAsync(u => u.Id == usuario.Id);
        var validationResult = ValidateInputUserData(usuario);
        var validatePermisos = ValidatePermisos(adminValidate);


        if (!validationResult.Success) return validationResult;
        if (!validateExistence) return new OperationResult  (false,  "El usuario que intenta editar no existe." );
        if (!validatePermisos.Success) return validatePermisos;

        try
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return new OperationResult(true, "Usuario actualizado con éxito.");

        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error de base de datos: {ex.Message}");
        }
    }
    
    // desactivar usuario, no se puede borrar

    public async Task<OperationResult> DeactivateUsuarioAsync(int id, int adminId)
    {
        // 1. Evitar suicidio administrativo
        if (id == adminId)
            return new OperationResult(false, "No puedes desactivar tu propia cuenta.");

        // 2. Validar permisos del que ejecuta
        var admin = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == adminId);

        if (admin == null || !admin.Rol.Permisos.HasFlag(RolModel.PermisosSistema.GestionarUsuarios))
            return new OperationResult(false, "No tienes permisos para realizar esta acción.");

        // 3. Buscar y desactivar
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return new OperationResult(false, "El usuario no existe.");

        try
        {
            // En lugar de _context.Remove(usuario), cambiamos un estado
            usuario.IsActive = false;
        
            // Si DE VERDAD quieres borrarlo físicamente (no recomendado):
            //_context.Usuarios.Remove(usuario);
        
            await _context.SaveChangesAsync();
            return new OperationResult(true, "Usuario procesado con éxito.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error: No se puede eliminar porque tiene historial vinculado.");
        }
    }
    
    // activar usuario otra vez

    public async Task<OperationResult> ActivateUsuarioAsync(int id, int adminId)
    {
        var admin = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == adminId);

        if (admin == null || !admin.Rol.Permisos.HasFlag(RolModel.PermisosSistema.GestionarUsuarios))
            return new OperationResult(false, "No tienes permisos para realizar esta acción.");

        // 3. Buscar y desactivar
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return new OperationResult(false, "El usuario no existe.");

        try
        {
            usuario.IsActive = true;
            await _context.SaveChangesAsync();
            return new OperationResult(true, "Usuario procesado con éxito.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Error: No se puede eliminar porque tiene historial vinculado.");
        }
    }
    
    // Obtener una lista con todos los usuarios

    public async Task<ListOperationResult<UsuarioModel>> GetAllUsuariosAsync(int adminId)
    {
        var adminValidate = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == adminId);

        var Permisos = ValidatePermisos(adminValidate);

        if (!Permisos.Success)
            return new ListOperationResult<UsuarioModel>(false, "No tiene los permisos necesarios", null);

        try
        {
            var UserList = await _context.Usuarios.Include(u => u.Rol)
                .AsNoTracking()
                .ToListAsync();

            return new ListOperationResult<UsuarioModel>(true, "", UserList);

        }
        catch (Exception e)
        {
            return new ListOperationResult<UsuarioModel>(false, $" Error: {e.Message}", null);
        }
        
    }
    
    
    //Metodos de uso multiple:
    
    public OperationResult ValidateInputUserData(UsuarioModel user)
    {
        if (string.IsNullOrWhiteSpace(user.Nombre))
        {
            return new OperationResult (false, "El nombre de usuario no puede estar vacío." );
        }


        if (string.IsNullOrWhiteSpace(user.Username))
        {
            return new OperationResult(false, "El nombre de usuario no puede estar vacío.");
        }


        if (string.IsNullOrWhiteSpace(user.Contrasena))
        {
            return new OperationResult(false, "La contraseña no puede estar vacía.");
        }


        if (user.RolId <= 0)
        {
            return new OperationResult(false, "Debe asignar un rol válido al usuario.");
        }

        return new OperationResult ( true, " "  );

    }

    //validar datos para actualizar usuario
	
    public OperationResult ValidatePermisos( UsuarioModel adminValidate)
    {
        if (adminValidate == null)
        {
            return new OperationResult ( false,  "Usuario administrador no encontrado." );
        }

        if (!adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.GestionarUsuarios))
        {
            return new OperationResult(false, "El usuario no tiene permisos para gestionar usuarios.");

        }
		
        return new OperationResult ( true, " " );
    }

    // Metodos de contrasena

    public string HashPassword(string Pass)
    {
        //por implementar
        return PasswordHasher.PasswordHash(Pass);

    }


    public bool VerifyPassword(string UserPass, string HashPass )
    {
        //por implementar
        return PasswordHasher.VerifyPassword(UserPass, HashPass);
    }
}
    
