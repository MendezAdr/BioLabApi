namespace BioLabApi.Models.DTOs
{
    public class DetalleResponseDTO
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public int ExamenId { get; set; }

        public decimal PrecioMomentoDivisa { get; set; } = 0;

    }
}
