using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioLabApi.Models;
using BioLabApi.Helpers;

namespace BioLabApi.Services.Interfaces;

public interface IPacientesService
{
    Task<ListOperationResult<PacienteModel>> GetAllPacientesAsync();
    Task<ObjectOperationResult> GetPacienteByIdAsync(int id);
    Task<ObjectOperationResult> GetByNombreAsync(string nombre);
    Task<ObjectOperationResult> GetByApellidoAsync(string apellido);
    Task<ObjectOperationResult> GetByCedulaAsync(string cedula);
    
    Task<OperationResult> CreateAsync(PacienteModel paciente, int userId);
    Task<OperationResult> UpdateAsync(PacienteModel paciente, int userId);
    Task<OperationResult> DeactivateAsync(int id, int adminId);
    Task<OperationResult> ActivateAsync(int id, int adminId, bool state);
    
    
}