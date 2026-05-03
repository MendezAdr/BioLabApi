using  BioLabProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BioLabProject.Services.Interfaces;

public interface IDetalleService
{
    Task<ObjectOperationResult> GetDetalleByIdAsync(int id);
    Task<ObjectOperationResult> GetDetalleByExamenIdAsync(int id);
    Task<ListOperationResult<DetalleModel>> GetDetallesByOrdenIdAsync(int id);

    Task<OperationResult> CreateDetalleAsync(DetalleModel detalle);
    Task<OperationResult> UpdateDetalleAsync(DetalleModel detalle, int AdminId);
    
    
}