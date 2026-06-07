using  BioLabApi.Models;
using BioLabApi.Helpers;
using BioLabApi.Models.DTOs;

namespace BioLabApi.Services.Interfaces;

public interface IDetalleService
{
    Task<ObjectOperationResult> GetDetalleByIdAsync(int id); //es preferible la orden con los detalles

    Task<ObjectOperationResult> GetDetalleByExamenIdAsync(int id); //inutil creo
    Task<ListOperationResult<DetalleResponseDTO>> GetDetallesByOrdenIdAsync(int id); //este si

    Task<OperationResult> CreateDetalleAsync(DetalleCreateDTO detalle); //este si   
    Task<OperationResult> UpdateDetalleAsync(DetalleUpdateDTO detalle, int AdminId, int detalleId); //este... tal vez no, no se pueden actualizar los detalles, si se quiere cambiar un examen en una orden, se borra el detalle y se crea uno nuevo con el nuevo examen


}