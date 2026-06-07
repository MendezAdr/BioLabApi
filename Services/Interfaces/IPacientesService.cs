using BioLabApi.Models;
using BioLabApi.Helpers;
using BioLabApi.Models.DTOs;

namespace BioLabApi.Services.Interfaces;

public interface IPacientesService
{
    Task<ListOperationResult<PacienteResponseDTO>> GetAllPacientesAsync();
    Task<ObjectOperationResult> GetPacienteByIdAsync(int id);
    Task<ObjectOperationResult> GetByNombreAsync(string nombre);
    Task<ObjectOperationResult> GetByApellidoAsync(string apellido);
    Task<ObjectOperationResult> GetByCedulaAsync(string cedula);
    
    Task<OperationResult> CreateAsync(PacienteCreateDTO paciente, int userId);
    Task<OperationResult> UpdateAsync(PacienteUpdateDTO paciente, int userId);
    Task<OperationResult> DeactivateAsync(int id, int adminId);
    Task<OperationResult> ActivateAsync(int id, int adminId, bool state);
    
    
}