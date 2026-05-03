using System;
using System.Collections.Generic;

namespace BioLabProject.Models;

public class OrdenesModel
{
    /*
     * Una orden representa una venta realizada a un paciente, 
     * que puede incluir múltiples exámenes (a través de Detalle)
     * y múltiples pagos (Multimoneda/Multitotal).
     */

    public int Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty; 
    public DateTime Fecha { get; set; } = DateTime.Now;
    
    // Relación con Paciente
    public int PacienteId { get; set; }
    public PacienteModel Paciente { get; set; } = null!;

    // Totales de la Orden
    public decimal TotalDivisa { get; set; }
    public decimal TasaBcv { get; set; } // La tasa del día de la venta
    public decimal TotalBs => TotalDivisa * TasaBcv; // Propiedad calculada

    // 1. Una Orden tiene muchos Exámenes (a través de Detalle)
    public List<DetalleModel> Detalles { get; set; } = new();

    // 2. Una Orden tiene muchos Pagos (Multimoneda/Multitotal)
    public List<PagosModel> Pagos { get; set; } = new();

    public EstadoPago estado { get; set; } // Pagado, Parcial, Pendiente

    [Flags]
    public enum EstadoPago
    {
        Pagado = 1,
        Pendiente = 2,
        Parcial = 3
    }
}