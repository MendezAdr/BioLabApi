using BioLabApi.Models;
using BioLabApi.Services.Interfaces;
using BioLabApi.Data;
using Microsoft.EntityFrameworkCore;
using BioLabApi.Helpers;
using BioLabApi.Models.DTOs;


namespace BioLabApi.Services.Servicios;

public class PagosService : IPagosService
{
    private readonly AppDbContext _dbContext;
    public PagosService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
     //obtener todos los pagos
    public async Task<ListOperationResult<PagoResponseDTO?>> GetAllPagosAsync()
    {
        try { 
            var pagos = await _dbContext.Pagos
                .Select(p => new PagoResponseDTO
                {
                    Id = p.Id,
                    OrdenId = p.OrdenId,
                    Metodo = p.Metodo,
                    Monto = p.Monto,
                    Referencia = p.Referencia
                })
                .AsNoTracking()
                .ToListAsync();
            return new ListOperationResult<PagoResponseDTO?> (true, "",  pagos );

        }
        catch (Exception ex)
        {
            return new ListOperationResult<PagoResponseDTO?> (  false,  $"Error al obtener los pagos: {ex.Message}",  null );
        }
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
            return new ObjectOperationResult(true, "", new PagoResponseDTO
            {
                Id = pago.Id,
                OrdenId = pago.OrdenId,
                Metodo = pago.Metodo,
                Monto = pago.Monto,
                Referencia = pago.Referencia
            });
        }
        catch (Exception e)
        {
            return new ObjectOperationResult(false, $"Error: {e.Message} ", null);
        }
    }

    public async Task<ObjectOperationResult> GetPagoByReferenciaAsync(string ReferenciaId)
    {
        var pago = await _dbContext.Pagos
            .Include(x => x.Orden)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Referencia.Equals(ReferenciaId));
        if (pago == null) return new ObjectOperationResult(false, "Error, no existen pagos asociados a esa referencia", null);
        try
        {
            return new ObjectOperationResult(true, "", new PagoResponseDTO
            {
                Id = pago.Id,
                OrdenId = pago.OrdenId,
                Metodo = pago.Metodo,
                Monto = pago.Monto,
                Referencia = pago.Referencia
            });
        }
        catch (Exception e)
        {
            return new ObjectOperationResult(false, $"Error: {e.Message} ", null);
        }

    }

    // obtener todos los pagos filtrados por metodo
    public async Task<ListOperationResult<PagoResponseDTO?>> GetPagosByMetodoAsync(int IdMetodo)
    {
        var listaPagos = await _dbContext.Pagos
            .Include(x => x.Orden)
            .AsNoTracking()
            .Where(p => p.Metodo.Equals(IdMetodo))
            .ToListAsync();
        if (listaPagos == null) return new ListOperationResult<PagoResponseDTO?>(false, "Error, aun no existen pagos por ese metodo", null);
        try
        {
            var pagoResponses = listaPagos.Select(p => new PagoResponseDTO
            {
                Id = p.Id,
                OrdenId = p.OrdenId,
                Metodo = p.Metodo,
                Monto = p.Monto,
                Referencia = p.Referencia
            }).ToList();

            return new ListOperationResult<PagoResponseDTO?>(true, "", pagoResponses);
        }
        catch (Exception e)
        {
            return new ListOperationResult<PagoResponseDTO?>(false, $"Error: {e.Message} ", null);
        }
    }

    //obtener los pagos asociados a una orden
    public async Task<ListOperationResult<PagoResponseDTO>> GetPagosByOrdenAsync(int OrdenId)
    {
        var listaPagos = await _dbContext.Pagos
            .Include(x => x.Orden)
            .AsNoTracking()
            .Where(p => p.OrdenId == OrdenId)
            .ToListAsync();
        try
        {
            if (listaPagos == null)
            {
                return new ListOperationResult<PagoResponseDTO>(false, "Error, no existen pagos asociados a esa orden", null);
            }

            var pagosResponseDTOs = listaPagos.Select(p => new PagoResponseDTO
            {
                Id = p.Id,
                Monto = p.Monto,
                Metodo = p.Metodo,
                Referencia = p.Referencia
            }).ToList();
            return new ListOperationResult<PagoResponseDTO>(true, "", pagosResponseDTOs);

        }
        catch (Exception e)
        {
            return new ListOperationResult<PagoResponseDTO>(false, $"Error: {e.Message} ", null);
        }
    }

    //obtener los pagos entre dos fechas
    public async Task<ListOperationResult<PagoResponseDTO>> GetAllPagosEntreFechasAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var listaPagos = await _dbContext.Pagos
            .Include(x => x.Orden)
            .AsNoTracking()
            .Where(p => p.Orden.Fecha >= fechaInicio && p.Orden.Fecha <= fechaFin)
            .ToListAsync();
        try
        {
            var pagosResponseDTOs = listaPagos.Select(p => new PagoResponseDTO          
            {
                Id = p.Id,
                Monto = p.Monto,
                Metodo = p.Metodo,
                Referencia = p.Referencia
            }).ToList();

            return new ListOperationResult<PagoResponseDTO>(true, "", pagosResponseDTOs);
        }
        catch (Exception e)
        {
            return new ListOperationResult<PagoResponseDTO>(false, $"Error: {e.Message} ", null);
        }
    }


    //metodos restantes

    public async Task<OperationResult> CreateAddPagoAsync(PagoStandaloneCreateDTO pago)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Agregamos el pago
            await _dbContext.Pagos.AddAsync(new PagosModel
            {
                OrdenId = pago.OrdenId,
                Monto = pago.Monto,
                Referencia = pago.Referencia,
                Metodo = pago.Metodo
            });

            // CRÍTICO: Necesitamos cargar la orden y TODOS sus pagos (incluyendo el nuevo) para recalcular
            var orden = await _dbContext.Ordenes
                .Include(o => o.Pagos)
                .FirstOrDefaultAsync(o => o.Id == pago.OrdenId);

            if (orden != null)
            {
                SincronizarPagosConOrden(orden);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OperationResult(true, "Pago registrado y orden sincronizada con éxito.");
        }
        catch (Exception e)
        {
            return new OperationResult(false, $"Error: {e.Message} ");
        }
    }


    public async Task<OperationResult> UpdatePagoAsync(PagoUpdateDTO pago, int adminId)
    {
        var admin = await _dbContext.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == adminId);

        var validPermisos = ValidatePermisos(admin);
        if (!validPermisos.Success) return validPermisos;


        // Búsqueda del pago INCLUYENDO la orden y todos los pagos hermanos
        var pagoDb = await _dbContext.Pagos
            .Include(p => p.Orden)
            .ThenInclude(o => o.Pagos) // <-- Vital para el recálculo
            .FirstOrDefaultAsync(p => p.Id == pago.Id);

        if (pagoDb == null) return new OperationResult(false, "El pago no existe.");

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Actualización quirúrgica
            pagoDb.Monto = pago.Monto;
            pagoDb.Referencia = pago.Referencia;
            pagoDb.Metodo = pago.Metodo;

            // Recálculo automático reutilizando tu método
            SincronizarPagosConOrden(pagoDb.Orden);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
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

        var pagoDb = await _dbContext.Pagos
            .Include(p => p.Orden)
            .ThenInclude(o => o.Pagos) // <-- CRÍTICO: Faltaba esto en tu código original
            .FirstOrDefaultAsync(p => p.Id == idPago);

        if (pagoDb == null) return new OperationResult(false, "El pago no existe.");

        try
        {
            pagoDb.Monto = 0;
            pagoDb.Referencia = $"ANULADO - {pagoDb.Referencia}";

            // Recálculo automático
            SincronizarPagosConOrden(pagoDb.Orden);

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

        if (!adminValidate.Rol.Permisos.HasFlag(RolModel.PermisosSistema.GestionarPagos))
        {
            return new OperationResult(false, "El usuario no tiene permisos para gestionar pagos.");
        }

        return new OperationResult(true, " ");
    }


    public ObjectOperationResult SincronizarPagosConOrden(OrdenesModel orden)
    {
        decimal totalPagadoDivisa = 0;
        foreach (var pago in orden.Pagos)
        {
            bool esBs = (int)pago.Metodo >= 1 && (int)pago.Metodo <= 4;
            totalPagadoDivisa += esBs ? (pago.Monto / orden.TasaBcv) : pago.Monto;
        }
        if (Math.Round(totalPagadoDivisa, 2) >= Math.Round(orden.TotalDivisa, 2))
            orden.Estado = OrdenesModel.EstadoPago.Pagado;
        else if (totalPagadoDivisa > 0)
            orden.Estado = OrdenesModel.EstadoPago.Parcial;
        else
            orden.Estado = OrdenesModel.EstadoPago.Pendiente;
        return new ObjectOperationResult(true, "", orden);

    }

}