using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioLabApi.Models;
using BioLabApi.Services.Interfaces;
using BioLabApi.Data;
using Microsoft.EntityFrameworkCore;
using BioLabApi.Helpers;


namespace BioLabApi.Services.Servicios;

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
            var pago = await _dbContext.Pagos
                .Include(x => x.Orden)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
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
            .Include (x => x.Orden)
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
            .Include(x => x.Orden)
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
            .Include (x => x.Orden)
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
            .Include(x => x.Orden)
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
        // 1. Validar Permisos del Administrador
        var admin = await _dbContext.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == adminId);

        var validPermisos = ValidatePermisos(admin);
        if (!validPermisos.Success) return validPermisos;

        // 2. Validar los datos del objeto pago
        var validPago = validatePago(pago); 
        if (!validPago.Success) return validPago;

        // 3. Buscar el pago existente INCLUYENDO la orden y todos sus pagos hermanos
        var pagoDb = await _dbContext.Pagos
            .Include(p => p.Orden)
            .ThenInclude(o => o.Pagos)
            .FirstOrDefaultAsync(p => p.Id == pago.Id);

        if (pagoDb == null) return new OperationResult(false, "El pago no existe.");

        try
        {
            // 4. Actualizar los campos permitidos
            pagoDb.Monto = pago.Monto;
            pagoDb.Referencia = pago.Referencia;
            pagoDb.Metodo = pago.Metodo;

            // 5. RECALCULAR EL ESTADO DE LA ORDEN
            decimal totalPagadoDivisa = 0;
            foreach (var p in pagoDb.Orden.Pagos)
            {
                bool esBs = (int)p.Metodo >= 1 && (int)p.Metodo <= 4;
                totalPagadoDivisa += esBs ? (p.Monto / pagoDb.Orden.TasaBcv) : p.Monto;
            }

            if (Math.Round(totalPagadoDivisa, 2) >= Math.Round(pagoDb.Orden.TotalDivisa, 2))
                pagoDb.Orden.Estado = OrdenesModel.EstadoPago.Pagado;
            else if (totalPagadoDivisa > 0)
                pagoDb.Orden.Estado = OrdenesModel.EstadoPago.Parcial;
            else
                pagoDb.Orden.Estado = OrdenesModel.EstadoPago.Pendiente;

            await _dbContext.SaveChangesAsync();
            return new OperationResult(true, "Pago actualizado y orden recalculada con éxito.");
        }
        catch (Exception e)
        {
            return new OperationResult(false, $"Error al actualizar: {e.Message}");
        }
    }

    
    public async Task<OperationResult> AnulatePagosAsync(int idPago, int adminId)
    {
        var admin = await _dbContext.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == adminId);
        var validPermisos = ValidatePermisos(admin);
        if (!validPermisos.Success) return validPermisos;

        var pagoDb = await _dbContext.Pagos.FirstOrDefaultAsync(p => p.Id == idPago);
        if (pagoDb == null) return new OperationResult(false, "El pago no existe.");

        try
        {
            pagoDb.Monto = 0;
            pagoDb.Referencia = $"ANULADO - {pagoDb.Referencia}";
        
            // Al igual que en Update, deberías llamar a la lógica para recalcular el estado de la Orden aquí
            // (Puedes extraer ese bloque de recálculo a un método privado para reutilizarlo)

            await _dbContext.SaveChangesAsync();
            return new OperationResult(true, "Pago anulado correctamente.");
        }
        catch (Exception e)
        {
            return new OperationResult(false, $"Error al anular: {e.Message}");
        }
    }


    // metodos de validacion

    public OperationResult validatePago(PagosModel pago)
    {   
        // OrdenId es int, su valor por defecto es 0 si no se asigna
        if (pago.OrdenId <= 0) 
            return new OperationResult(false, "No puede registrar un pago sin una orden válida.");
    
        // Monto es decimal, verificamos que sea mayor a 0
        if (pago.Monto <= 0) 
            return new OperationResult(false, "Inserte un monto válido mayor a cero.");
        
        // La referencia no puede estar vacía
        if (string.IsNullOrWhiteSpace(pago.Referencia)) 
            return new OperationResult(false, "Inserte una referencia válida.");
    
        // Enum Metodo siempre tiene un valor por defecto, podríamos validar que esté en el rango si es necesario
        if (!Enum.IsDefined(typeof(PagosModel.MetodoPago), pago.Metodo))
            return new OperationResult(false, "El método de pago seleccionado no es válido.");
    
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