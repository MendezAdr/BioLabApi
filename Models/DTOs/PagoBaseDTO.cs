// DTOs/PagoBaseDTO.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BioLabApi.Models.DTOs
{
    // Clase abstracta: contiene lo que TODOS los pagos comparten
    public abstract class PagoBaseDTO : IValidatableObject
    {
        [Required]
        [Range(0.01, 9999999.99, ErrorMessage = "Inserte un monto válido mayor a cero en el pago.")]
        public decimal Monto { get; set; }

        [Required]
        [Range(1, 6, ErrorMessage = "No puedes registrar un pago sin especificar un método válido.")]
        public PagosModel.MetodoPago Metodo { get; set; }

        [MaxLength(80)]
        public string Referencia { get; set; } = string.Empty;

        // La validación condicional se queda en la clase base para no repetirla
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            bool esPagoDigital = Metodo == PagosModel.MetodoPago.PagoMovil ||
                                 Metodo == PagosModel.MetodoPago.Transferencia;

            if (esPagoDigital && string.IsNullOrWhiteSpace(Referencia))
            {
                yield return new ValidationResult(
                    "Los pagos digitales requieren una referencia obligatoria.",
                    new[] { nameof(Referencia) }
                );
            }
        }
    }
}