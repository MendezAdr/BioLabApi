
namespace BioLabApi.Models;

public class RolModel
{
    public int Id { get; set; }
    public string RolName { get; set; } = string.Empty;
    public List<UsuarioModel> Usuarios { get; set; } = new();
    public PermisosSistema Permisos { get; set; }

    [Flags] // importante, colocar mas permisos, ser mas especifico.
    public enum PermisosSistema
    {
        Ninguno = 0,
        CrearVenta = 1,      
        HacerCierre = 2,     
        GestionarUsuarios = 4, 
        VerReportesAntiguos = 8,
        ModificarExamenes = 16,
        ModificarPacientes = 32,
        ModificarPagos = 64,
        ModificarOrdenesYDetalles = 128,

    }
}