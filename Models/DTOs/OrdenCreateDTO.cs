using System.ComponentModel.DataAnnotations;

namespace BioLabApi.Models.DTOs
{
    public class OrdenCreateDTO
    {
        [Required]
        [MaxLength(80)]
        public string NumeroFactura { get; set; } = string.Empty;

        [Required]
        public int PacienteId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El total en divisa debe ser un valor positivo.")]
        public decimal TotalDivisa { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La tasa BCV del día debe ser un valor positivo.")]
        public decimal TasaBcv { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // LISTAS ANIDADAS USANDO DTOs
        [Required]
        [MinLength(1, ErrorMessage = "La orden debe contener al menos un examen (detalle).")]
        public List<DetalleCreateDTO> Detalles { get; set; } = new();

        // Los pagos pueden ir vacíos si la orden queda "Pendiente" sin abonos iniciales
        public List<PagoOrdenCreateDTO> Pagos { get; set; } = new();
    }
}
