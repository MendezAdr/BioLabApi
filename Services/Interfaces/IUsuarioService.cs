using BioLabApi.Models;
using BioLabApi.Helpers;
using BioLabApi.Models.DTOs;


namespace BioLabApi.Services.Interfaces;

public interface IUsuarioService
{
    Task<ObjectOperationResult?> LoginAsync(string username, string password);
    
    Task<OperationResult?> LogOutAsync();

    Task<OperationResult> CreateUsuarioAsync(UsuarioCreateDTO usuario, int adminId);

    Task<OperationResult> UpdateUsuarioAsync(UsuarioUpdateDTO usuario, int adminId);

    Task<OperationResult> ChangePasswordAsync(int usuarioId, string password, int adminId);

    Task<OperationResult> DeactivateUsuarioAsync(int Id, int adminId);

    Task<OperationResult> ActivateUsuarioAsync(int Id, int adminId);

    Task<ObjectOperationResult> GetUserByIdAsync(int id, int adminId);

    Task<ListOperationResult<UsuarioResponseDTO>> GetAllUsuariosAsync(int adminId);


}