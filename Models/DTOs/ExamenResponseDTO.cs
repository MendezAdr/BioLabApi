namespace BioLabApi.Models.DTOs
{
    public class ExamenResponseDTO : AuditableResponseDTO
    {
        public int Id { get; set; }
        public string NombreExamen { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal CostoEnDivisa { get; set; }
        
    }
}
