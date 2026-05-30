// DTOs/UsuarioUpdateDTO.cs
using System.ComponentModel.DataAnnotations;

namespace BioLabApi.Models.DTOs;

public class UsuarioUpdateDTO
{
    // OJO: Al actualizar, SÍ necesitamos recibir el ID para saber a quién modificar
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [MaxLength(20)]
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
    public int RolId { get; set; }
}