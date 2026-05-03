using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioLabApi.Models;

namespace BioLabApi.Services.Interfaces;

public interface IOrdenesService
{
    // Búsquedas y filtrados
    Task<ObjectOperationResult> GetOrdenByIdAsync(int id, int AdminId);
    Task<ListOperationResult<Ordenes>> GetAllOrdenesAsync(int AdminId);
    Task<ListOperationResult<Ordenes>> GetAllOrdenesByPacienteAsync(int idPaciente, int AdminId);
    Task<ListOperationResult<Ordenes>> GetAllOrdenesEntreFechasAsync(DateTime inicio, DateTime fin, int AdminId);
    Task<ListOperationResult<Ordenes>> GetAllOrdenesByEstadoAsync(string estado, int AdminId);

    // Operaciones de escritura
    Task<OperationResult> CreateOrdenAsync(Ordenes orden);
    Task<OperationResult> UpdateEstadoOrdenAsync(int id, string nuevoEstado, int AdminId);
    Task<OperationResult> DeactivateOrdenAsync(int id, int AdminId);
}