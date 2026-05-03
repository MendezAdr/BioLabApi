using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioLabProject.Models;

namespace BioLabProject.Services.Interfaces;

public interface IPacientesService
{
    Task<ListOperationResult<PacienteModel>> GetAllPacientesAsync();
    Task<ObjectOperationResult> GetPacienteByIdAsync(int id);
    Task<ObjectOperationResult> GetByNombreAsync(string nombre);
    Task<ObjectOperationResult> GetByApellidoAsync(string apellido);
    Task<ObjectOperationResult> GetByCedulaAsync(string cedula);
    
    Task<OperationResult> CreateAsync(PacienteModel paciente);
    Task<OperationResult> UpdateAsync(PacienteModel paciente);
    Task<OperationResult> DeactivateAsync(int id, int adminId);
    Task<OperationResult> ActivateAsync(int id, int adminId);
    
    
}