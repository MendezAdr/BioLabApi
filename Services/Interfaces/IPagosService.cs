using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioLabProject.Models;

namespace BioLabProject.Services.Interfaces;

public interface IPagosService
{   
    //obtencion de pagos
    Task<ObjectOperationResult> GetPagoByIdAsync(int id);
    Task<ListOperationResult<PagosModel>> GetPagosByMetodoAsync(int IdMetodo);
    Task<ListOperationResult<PagosModel>> GetPagosByOrdenAsync(int OrdenId);
    Task<ObjectOperationResult> GetPagoByReferenciaAsync(string ReferenciaId);
    Task<ListOperationResult<PagosModel>> GetAllPagosEntreFechasAsync(DateTime? fechaInicio, DateTime? fechaFin );
    
    
    //metodos restantes
    Task<OperationResult> CreatePagoAsync(PagosModel pago);
    Task<OperationResult> UpdatePagoAsync(PagosModel pago, int adminId);
    Task<OperationResult> DeactivatePagoAsync(int idPago, int adminId);
    Task<ListOperationResult<PagosModel>> DeactivatePagosAsync(List<int> pagos, int adminId); 
    Task<OperationResult> ActivatePagoAsync(int idPago, int adminId);
    


}