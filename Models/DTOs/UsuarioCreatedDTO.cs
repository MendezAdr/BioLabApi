// DTOs/UsuarioCreateDTO.cs
using System.ComponentModel.DataAnnotations;

namespace BioLabApi.Models.DTOs;

public class UsuarioCreateDTO
{
    // Usamos DataAnnotations en el DTO para validar antes de que llegue al servicio
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El nombre de usuario no puede exceder los 20 caracteres.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)]
    public string Cedula { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;


    [Required]
    public int RolId { get; set; }
}