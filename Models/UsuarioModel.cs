using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioLabApi.Models;
public class UsuarioModel : Auditable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [MaxLength(20)]
    public string Username { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Apellido { get; set; } = string.Empty;
    [MaxLength(10)]
    public string Cedula { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Relación explícita
    public int RolId { get; set; } 
    public RolModel Rol { get; set; } = null!;

}