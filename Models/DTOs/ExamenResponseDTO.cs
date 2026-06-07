namespace BioLabApi.Models.DTOs
{
    public class ExamenResponseDTO
    {
        public int Id { get; set; }
        public string NombreExamen { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal CostoEnDivisa { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int CreadoPorId { get; set; }
        public int? ModificadoPorId { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
