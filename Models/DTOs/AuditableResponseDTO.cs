
namespace BioLabApi.Models.DTOs
{
    public class AuditableResponseDTO
    {
        public int CreadoPorId { get; set; } = 0;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public int? ModificadoPorId { get; set; } = null;

        public DateTime? FechaModificacion { get; set; } = null;
        

    }
}
