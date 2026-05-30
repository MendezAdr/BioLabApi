using System.ComponentModel.DataAnnotations;

namespace BioLabApi.Models.DTOs
{
    public class DetalleUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El ID de la orden es requerido")]
        public int OrdenId { get; set; }

        [Required(ErrorMessage = "El ID del examen es requerido")]
        public int ExamenId { get; set; }

        [Required(ErrorMessage = "El precio del momento es requerido")]
        public decimal PrecioMomentoDivisa { get; set; }
    }
}
