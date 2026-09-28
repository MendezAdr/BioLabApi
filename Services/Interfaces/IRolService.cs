using BioLabApi.Models.DTOs;
using BioLabApi.Helpers;
namespace BioLabApi.Services.Interfaces
{
    public interface IRolService
    {
        Task<ListOperationResult<RolResponseDTO>> GetAllRolesAsync();
        Task<ObjectOperationResult> GetRolByIdAsync(int id);
        Task<ObjectOperationResult> CreateRolAsync(RolCreateDTO rolCreateDto);
        Task<ObjectOperationResult> UpdateRolAsync(RolUpdateDto rolUpdateDto);
        Task<OperationResult> DeleteRolAsync(int id);
    }
}
