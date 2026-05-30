using System.ComponentModel.DataAnnotations;

namespace BioLabApi.Models.DTOs
{
    public class DetalleCreateDTO
    {
        

        [Required(ErrorMessage = "El ID del examen es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe haber al menos un examen válido para cada detalle.")]
        public int ExamenId { get; set; }

        [Required(ErrorMessage = "El precio del momento es requerido")]
        [Range(0, 9999999.99, ErrorMessage = "El precio del examen no puede ser negativo.")]
        public decimal PrecioMomentoDivisa { get; set; }
    }
}
