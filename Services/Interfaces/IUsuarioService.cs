using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioLabApi.Models;
using BioLabApi.Services.Interfaces;


namespace BioLabApi.Services.Interfaces;

public interface IUsuarioService
{
    Task<ObjectOperationResult?> LoginAsync(string username, string password);
    
    Task<OperationResult?> LogOutAsync();

    Task<OperationResult> CreateUsuarioAsync(UsuarioModel usuario, int adminId);

    Task<OperationResult> UpdateUsuarioAsync(UsuarioModel usuario, int adminId);

    Task<OperationResult> ChangePasswordAsync(int usuarioId, string password, int adminId);

    Task<OperationResult> DeactivateUsuarioAsync(int Id, int adminId);

    Task<OperationResult> ActivateUsuarioAsync(int Id, int adminId);

    Task<ObjectOperationResult> GetUserByIdAsync(int id, int adminId);

    Task<ListOperationResult<UsuarioModel>> GetAllUsuariosAsync(int adminId);


}