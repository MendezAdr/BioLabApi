using BioLabApi.Helpers;

namespace BioLabApi.Services.Interfaces;

public interface IPagosService
{   
    //obtencion de pagos
    Task<ListOperationResult<PagosModel?>> GetAllPagosAsync();
    Task<ObjectOperationResult> GetPagoByIdAsync(int id);
    Task<ListOperationResult<PagosModel>> GetPagosByMetodoAsync(int IdMetodo);
    Task<ListOperationResult<PagosModel>> GetPagosByOrdenAsync(int OrdenId);
    Task<ObjectOperationResult> GetPagoByReferenciaAsync(string ReferenciaId);
    Task<ListOperationResult<PagosModel>> GetAllPagosEntreFechasAsync(DateTime? fechaInicio, DateTime? fechaFin );
    
    
    //metodos restantes
    Task<OperationResult> CreatePagoAsync(PagosModel pago);
    Task<OperationResult> UpdatePagoAsync(PagosModel pago, int adminId);
    Task<OperationResult> AnulatePagosAsync(int idPago, int adminId);



}