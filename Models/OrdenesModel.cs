

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioLabApi.Models;

public class OrdenesModel : Auditable
{
    /*
     * Una orden representa una venta realizada a un paciente, 
     * que puede incluir múltiples exámenes (a través de Detalle)
     * y múltiples pagos (Multimoneda/Multitotal).
     */
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(80)]
    public string NumeroFactura { get; set; } = string.Empty; 
    public DateTime Fecha { get; set; } = DateTime.Now;

    // Relación con Paciente
    [Required]
    public int PacienteId { get; set; }
    public PacienteModel Paciente { get; set; } = null!;

    // Totales de la Orden
    [Required]
    public decimal TotalDivisa { get; set; }
    [Required]
    public decimal TasaBcv { get; set; } // La tasa del día de la venta
    public decimal TotalBs => TotalDivisa * TasaBcv; // Propiedad calculada


    // 1. Una Orden tiene muchos Exámenes (a través de Detalle)
    [Required]
    [MinLength(1, ErrorMessage = "La orden debe contener al menos un examen (detalle).")]
    public List<DetalleModel> Detalles { get; set; } = new();

    // 2. Una Orden tiene muchos Pagos (Multimoneda/Multitotal)
    
    public List<PagosModel> Pagos { get; set; } = new();

    [Required]
    public EstadoPago Estado { get; set; } // Pagado, Parcial, Pendiente

    
    public enum EstadoPago
    {
        Pagado = 1,
        Pendiente = 2,
        Parcial = 3,
        Anulada = 4
    }
}
