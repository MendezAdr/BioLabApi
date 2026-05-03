using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioLabApi.Models;
using BioLabApi.Services.Interfaces;

namespace BioLabApi.Services.Interfaces;

public interface IExamenesService 
{
    Task<ListOperationResult<ExamenModel>> GetExamenesAsync();
    Task<ObjectOperationResult> GetExamenByIdAsync(int id);
    Task<ObjectOperationResult> CreateExamenAsync(ExamenModel examen, int AdminId);

    Task<OperationResult> UpdateExamenAsync(ExamenModel examen, int AdminId, int ExamenId);
    Task<OperationResult> DeleteExamenAsync(int id, int AdminId); 

    
    
}