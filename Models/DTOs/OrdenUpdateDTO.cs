using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Asegúrate de tener este using

namespace BioLabApi.Models.DTOs
{
    public class OrdenUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string NumeroFactura { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El total en divisa debe ser un valor positivo.")]
        public decimal TotalDivisa { get; set; }

        // LISTAS ANIDADAS USANDO DTOs DE ACTUALIZACIÓN (ESTA ES LA CORRECCIÓN)
        [Required]
        [MinLength(1, ErrorMessage = "La orden debe contener al menos un examen (detalle).")]
        public List<DetalleUpdateDTO> Detalles { get; set; } = new();

        // Los pagos pueden ir vacíos si la orden queda "Pendiente" sin abonos iniciales
        public List<PagoUpdateDTO> Pagos { get; set; } = new();
    }
}