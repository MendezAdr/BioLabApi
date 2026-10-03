namespace BioLabApi.Models.DTOs
{
    public class DetalleResponseDTO : AuditableResponseDTO
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public int ExamenId { get; set; }
        public string ExamenNombre {get; set;} = string.Empty;

        public decimal PrecioMomentoDivisa { get; set; } = 0;

        

    }
}
