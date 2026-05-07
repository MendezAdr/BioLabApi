using BioLabApi.Models;

namespace BioLabApi.Models;

public abstract class Auditable
{
    // Campos de Auditoría
    public int CreadoPorId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public int? ModificadoPorId { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
