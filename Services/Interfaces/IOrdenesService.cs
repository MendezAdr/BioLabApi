using BioLabApi.Helpers;
using BioLabApi.Models;
using BioLabApi.Models.DTOs;

namespace BioLabApi.Services.Interfaces;

public interface IOrdenesService
{
    // Búsquedas y filtrados
    Task<ObjectOperationResult> GetOrdenByIdAsync(int id, int AdminId);
    Task<ListOperationResult<OrdenesModel>> GetAllOrdenesAsync(int AdminId);
    Task<ListOperationResult<OrdenesModel>> GetAllOrdenesByPacienteAsync(int idPaciente, int AdminId);
    Task<ListOperationResult<OrdenesModel>> GetAllOrdenesEntreFechasAsync(DateTime inicio, DateTime fin, int AdminId);
    Task<ListOperationResult<OrdenesModel>> GetAllOrdenesByEstadoAsync(OrdenesModel.EstadoPago estado, int AdminId);

    // Operaciones de escritura
    Task<OperationResult> CreateOrdenAsync(OrdenCreateDTO orden, int usuarioId);
    Task<OperationResult> UpdateEstadoOrdenAsync(int id, string nuevoEstado, int AdminId);
    Task<OperationResult> UpdateOrdenAsync(int id, OrdenUpdateDTO orden, int AdminId);

    Task<OperationResult> DeactivateOrdenAsync(int id, int AdminId);
}