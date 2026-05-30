
using BioLabApi.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PagosModel
{
    /*
     * se sobre entiende que un pago es una
     * manera de "pagar" una orden, por lo tanto, cada pago
     * tendra solo un metodo de pago, un monto y una referencia (si aplica)
     */
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public int OrdenId { get; set; } 
    public OrdenesModel Orden { get; set; } = null!;

    [Required]
    public MetodoPago Metodo { get; set; }
    [Required]
    public decimal Monto { get; set; }
    [Required]
    [MaxLength(80)]
    public string Referencia { get; set; } = string.Empty;

    public enum MetodoPago
    {
        Punto = 1, PagoMovil = 2, BioPago = 3, EfectivoBs = 4, Divisas = 5, Transferencia = 6
    }
}
