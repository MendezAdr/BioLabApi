using BioLabApi.Models;
using BioLabApi.Helpers;
using BioLabApi.Models.DTOs;

namespace BioLabApi.Services.Interfaces;

public interface IExamenesService 
{
    Task<ListOperationResult<ExamenResponseDTO>> GetExamenesAsync();
    Task<ObjectOperationResult> GetExamenByIdAsync(int id);
    Task<OperationResult> CreateExamenAsync(ExamenCreateDTO examen, int AdminId);

    Task<OperationResult> UpdateExamenAsync(ExamenUpdateDTO examen, int AdminId, int ExamenId);
    Task<OperationResult> DeleteExamenAsync(int id, int AdminId); 

    
    
}