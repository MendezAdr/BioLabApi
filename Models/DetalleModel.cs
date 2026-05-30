

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioLabApi.Models;
public class DetalleModel
{ /*
   * el detalle es la relacion entre una orden y un examen especifico,
   * es decir, cada detalle representa un examen que se vendio en una orden, 
   * y guarda el precio del momento por si el examen sube de precio mañana en la base de datos.
   */
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public int OrdenId { get; set; }
    public OrdenesModel Orden { get; set; } = null!;
    [Required]
    public int ExamenId { get; set; }
    public ExamenModel Examen { get; set; } = null!;

    // Guardamos el precio del momento por si el examen sube de precio mañana
    [Required]
    public decimal PrecioMomentoDivisa { get; set; } 
}