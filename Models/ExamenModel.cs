

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioLabApi.Models;

public class ExamenModel : Auditable
{
    //simplemente es el modelo basico para un examen

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(20)]
    public string NombreExamen { get; set; } = string.Empty;
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "El costo en divisa debe ser un valor positivo.")]
    public decimal CostoEnDivisa { get; set; }
    
    [MaxLength(150)] //es opcional, pero no puede ser muy largo
    public string Descripcion { get; set; } = string.Empty;
}
