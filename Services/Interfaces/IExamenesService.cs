using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioLabProject.Models;
using BioLabProject.Services.Interfaces;

namespace BioLabProject.Services.Interfaces;

public interface IExamenesService
{
    Task<ListOperationResult<ExamenModel>> GetExamenesAsync();
    Task<ObjectOperationResult> GetExamenByIdAsync(int id);
    Task<ObjectOperationResult> CreateExamenAsync(ExamenModel examen, int AdminId);

    Task<OperationResult> UpdateExamenAsync(ExamenModel examen, int AdminId, int ExamenId);
    Task<OperationResult> DeleteExamenAsync(int id, int AdminId); 

    
    
}