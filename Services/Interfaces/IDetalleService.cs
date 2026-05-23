using  BioLabApi.Models;
using BioLabApi.Helpers;

namespace BioLabApi.Services.Interfaces;

public interface IDetalleService
{
    Task<ObjectOperationResult> GetDetalleByIdAsync(int id); //es preferible la orden con los detalles

    Task<ObjectOperationResult> GetDetalleByExamenIdAsync(int id); //inutil creo
    Task<ListOperationResult<DetalleModel>> GetDetallesByOrdenIdAsync(int id); //este si

    Task<OperationResult> CreateDetalleAsync(DetalleModel detalle); //este si   
    Task<OperationResult> UpdateDetalleAsync(DetalleModel detalle, int AdminId, int detalleId); //este... tal vez no, no se pueden actualizar los detalles, si se quiere cambiar un examen en una orden, se borra el detalle y se crea uno nuevo con el nuevo examen


}