// DTOs/PagoUpdateDTO.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BioLabApi.Models.DTOs;

public class PagoUpdateDTO : IValidatableObject
{
    [Required]
    public int Id { get; set; } // 0 si es un nuevo abono, >0 si es una corrección

    [Required]
    [Range(0.01, 9999999.99, ErrorMessage = "Inserte un monto válido mayor a cero.")]
    public decimal Monto { get; set; }

    [Required]
    public PagosModel.MetodoPago Metodo { get; set; }

    [MaxLength(80)]
    public string Referencia { get; set; } = string.Empty;

    // Auto-validación condicional integrada (Sustituye por completo a ValidatePagos)
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