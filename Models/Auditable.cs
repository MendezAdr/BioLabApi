

using System.ComponentModel.DataAnnotations;

namespace BioLabApi.Models;

public abstract class Auditable
{
    // Campos de Auditoría

    [Required]
    public int CreadoPorId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    [Range(0, double.MaxValue, ErrorMessage = "El ID del usuario que modificó debe ser un valor positivo.")]
    public int? ModificadoPorId { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
