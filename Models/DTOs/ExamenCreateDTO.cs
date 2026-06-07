using System.ComponentModel.DataAnnotations;

namespace BioLabApi.Models.DTOs
{
    public class ExamenCreateDTO
    {
        [Required]
        [MaxLength(20)]
        public string NombreExamen { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El costo en divisa debe ser un valor positivo.")]
        public decimal CostoEnDivisa { get; set; }

        [MaxLength(150)] //es opcional, pero no puede ser muy largo
        public string Descripcion { get; set; } = string.Empty;
    }
}
