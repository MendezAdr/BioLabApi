using System.Globalization;

namespace BioLabApi.Models.DTOs
{
    public class OrdenResponseDTO
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public DateTime FechaOrden { get; set; }

        public string NumeroFactura { get; set; } = string.Empty;

        public decimal TotalDivisa { get; set; } = 0;

        public OrdenesModel.EstadoPago Estado { get; set; }

        public List<DetalleResponseDTO> Detalles { get; set; } = new();

        public List<PagoResponseDTO> Pagos { get; set; } = new();
    }
}
