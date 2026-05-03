using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioLabProject.Models;
using BioLabProject.Services.Interfaces;
using BioLabProject.Data;
using Microsoft.EntityFrameworkCore;


namespace BioLabProject.Services.Servicios;

public class PagosService : IPagosService
{
    private readonly AppDbContext _dbContext;
    public PagosService (AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    //obtener un pago especifico
    public async Task<ObjectOperationResult> GetPagoByIdAsync(int id)
    {
        try
        {
            var pago = await _dbContext.Pagos.FirstOrDefaultAsync(p => p.Id == id);
            if (pago == null) return new ObjectOperationResult(false, "Error, el pago buscado no existe", null);
            return new ObjectOperationResult(true, "", pago);
        }
        catch (Exception e)
        {
            return new ObjectOperationResult(false, $"Error: {e.Message} ", null);
        }
    }
    
    public async Task<ObjectOperationResult> GetPagoByReferenciaAsync(string ReferenciaId)
    {
        var pago = await  _dbContext.Pagos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Referencia.Equals(ReferenciaId));
        if (pago == null) return new ObjectOperationResult(false, "Error, no existen pagos asociados a esa referencia", null);
        try
        {
            return new ObjectOperationResult(true, "", pago);
        }
        catch (Exception e)
        {
            return new ObjectOperationResult(false, $"Error: {e.Message} ", null);
        }
        
    }
    // obtener todos los pagos filtrados por metodo
    public async Task<ListOperationResult<PagosModel>> GetPagosByMetodoAsync(int IdMetodo)
    {
        var listaPagos = await _dbContext.Pagos
            .AsNoTracking()
            .Where(p => p.Metodo.Equals(IdMetodo))
            .ToListAsync();
        if (listaPagos == null) return new ListOperationResult<PagosModel>(false, "Error, aun no existen pagos por ese metodo", null);
        try
        {
            return new ListOperationResult<PagosModel>(true, "", listaPagos);
        }
        catch (Exception e)
        {
            return  new ListOperationResult<PagosModel>(false, $"Error: {e.Message} ", null);
        }
    }
    
    //obtener los pagos asociados a una orden
    public async Task<ListOperationResult<PagosModel>> GetPagosByOrdenAsync(int OrdenId)
    {
        var listaPagos = await _dbContext.Pagos
            .AsNoTracking()
            .Where(p => p.OrdenId == OrdenId)
            .ToListAsync();
        try
        {
        if (listaPagos == null)
        {
            return new ListOperationResult<PagosModel>(false, "Error, no existen pagos asociados a esa orden", null);
        }
        return new ListOperationResult<PagosModel>(true, "", listaPagos);

        }
        catch (Exception e)
        {
            return  new ListOperationResult<PagosModel>(false, $"Error: {e.Message} ", null);
        }
    }

    //obtener los pagos entre dos fechas
    public async Task<ListOperationResult<PagosModel>> GetAllPagosEntreFechasAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var listaPagos = await _dbContext.Pagos
            .AsNoTracking()
            .Where(p => p.Orden.Fecha >= fechaInicio && p.Orden.Fecha <= fechaFin)
            .ToListAsync();
        try
        {
            return new ListOperationResult<PagosModel>(true, "", listaPagos);
        }
        catch (Exception e)
        {
            return new ListOperationResult<PagosModel>(false, $"Error: {e.Message} ", null);
        }
    }
    
    
    
    
    
    //metodos restantes
    public async Task<OperationResult> CreatePagoAsync(PagosModel pago)
    {
        var validPago = validatePago(pago);
        if (!validPago.Success) return validPago;
        
        try
        {
            await _dbContext.Pagos.AddAsync(pago);
            await _dbContext.SaveChangesAsync();
            return new OperationResult(true, "Pago registrado con exito.");

        }
        catch (Exception e)
        {
            return new OperationResult(false, $"Error: {e.Message} ");
        }
    }

    public async Task<OperationResult> UpdatePagoAsync(PagosModel pago, int adminId)
    {
        
    }

    public async Task<OperationResult> DeactivatePagoAsync(int idPago, int adminId)
    {
        
    }

    public async Task<ListOperationResult<PagosModel>> DeactivatePagosAsync(List<int> pagos, int adminId)
    {
        
    }

    public async Task<OperationResult> ActivatePagoAsync(int idPago, int adminId)
    {
        
    }

    
    // metodos de validacion

    public OperationResult validatePago(PagosModel pago)
    {   
        // ordenId
        if (pago.OrdenId == null) return new OperationResult(false, "no puede registrar un pago sin una orden");
        // fecha
        if (pago.Orden.Fecha < DateTime.Now) return new OperationResult(false, "La fecha de la orden no puede ser anterior a la actual");
        // metodo
        if (pago.Metodo == null) return new OperationResult(false, "No puedes registrar un pago sin especificar el metodo");
        // monto
        if (pago.Monto < 0 || decimal.IsNegative(pago.Monto) || pago.Monto == null) return new OperationResult(false, "Inserte un monto valido");
        // referencia
        if (string.IsNullOrEmpty(pago.Referencia)) return new OperationResult(false, "Inserte una referencia valida");
        
        return new OperationResult(true, "");
    }
    
    public OperationResult ValidatePermisos(UsuarioModel adminValidate)
    {
        if (adminValidate == null) return new OperationResult(false, "Usuario administrador no encontrado.");
        
        if (!adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.CrearVenta))
        {
            return new OperationResult(false, "El usuario no tiene permisos para gestionar ventas.");
        }
        
        return new OperationResult(true, " ");
    }

    
}