using BioLabProject.Models;

public class DetalleModel
{ /*
   * el detalle es la relacion entre una orden y un examen especifico,
   * es decir, cada detalle representa un examen que se vendio en una orden, 
   * y guarda el precio del momento por si el examen sube de precio mañana en la base de datos.
   */
    public int Id { get; set; }
    public int OrdenId { get; set; }
    public OrdenesModel Orden { get; set; } = null!;

    public int ExamenId { get; set; }
    public ExamenModel Examen { get; set; } = null!;

    // Guardamos el precio del momento por si el examen sube de precio mañana
    public decimal PrecioMomentoDivisa { get; set; } 
}