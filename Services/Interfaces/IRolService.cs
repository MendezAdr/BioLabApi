using BioLabApi.Models.DTOs;
namespace BioLabApi.Services.Interfaces
{
    public interface IRolService
    {
        Task<List<RolResponseDTO>> GetAllRolesAsync();
        Task<RolResponseDTO> GetRolByIdAsync(int id);
        Task<RolResponseDTO> CreateRolAsync(RolCreateDTO rolCreateDto);
        Task<RolResponseDTO> UpdateRolAsync(RolUpdateDto rolUpdateDto);
        Task<bool> DeleteRolAsync(int id);
    }
}
