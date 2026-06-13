using BioLabApi.Helpers;
using BioLabApi.Models.DTOs;

namespace BioLabApi.Services.Interfaces;

public interface IPagosService
{   
    //obtencion de pagos
    Task<ListOperationResult<PagoResponseDTO?>> GetAllPagosAsync();
    Task<ObjectOperationResult> GetPagoByIdAsync(int id);
    Task<ListOperationResult<PagoResponseDTO>> GetPagosByMetodoAsync(int IdMetodo);
    Task<ListOperationResult<PagoResponseDTO>> GetPagosByOrdenAsync(int OrdenId);
    Task<ObjectOperationResult> GetPagoByReferenciaAsync(string ReferenciaId);
    Task<ListOperationResult<PagoResponseDTO>> GetAllPagosEntreFechasAsync(DateTime? fechaInicio, DateTime? fechaFin );
    
    
    //metodos restantes

    Task<OperationResult> CreateAddPagoAsync(PagoStandaloneCreateDTO pago);
    Task<OperationResult> UpdatePagoAsync(PagoUpdateDTO pago, int adminId);
    Task<OperationResult> AnulatePagosAsync(int idPago, int adminId);



}