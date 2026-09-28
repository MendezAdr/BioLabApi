
namespace BioLabApi.Models.DTOs;


public class UsuarioResponseDTO
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty; 
    public string Apellido { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public int RolId {get;set;}
    public string RolName { get; set; } = string.Empty; // Solo envías el nombre del rol, no el objeto entero
    public bool IsActive { get; set; }
    public RolModel.PermisosSistema permisosSistema {get;set;}
}