namespace BioLabApi.Models.DTOs
{
    public class PagoResponseDTO
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }

        public PagosModel.MetodoPago Metodo { get; set; }

        public decimal Monto { get; set; } = 0;

        public string Referencia { get; set; } = string.Empty;
    }
}
