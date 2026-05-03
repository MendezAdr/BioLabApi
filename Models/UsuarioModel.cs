// modelo de usuario
using BioLabProject.Models;
using System;

namespace BioLabProject.Models;
public class UsuarioModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Relación explícita
    public int RolId { get; set; } 
    public RolModel Rol { get; set; } = null!;

}