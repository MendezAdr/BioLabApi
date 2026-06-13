// DTOs/PagoStandaloneCreateDTO.cs
using System.ComponentModel.DataAnnotations;

namespace BioLabApi.Models.DTOs
{
    public class PagoStandaloneCreateDTO : PagoBaseDTO
    {
        // En este contexto, el frontend SÍ está obligado a enviar a qué orden pertenece el pago
        [Required(ErrorMessage = "El ID de la orden es obligatorio para registrar un pago individual.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la orden no es válido.")]
        public int OrdenId { get; set; }
    }
}