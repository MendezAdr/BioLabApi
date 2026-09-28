
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
        GestionarExamenes = 1,
        GestionarPresupuestos = 2,
        GestionarPacientes = 4,
        GestionarPagos = 8,
        Totalizar = 16,
        CrearOrdenesYDetalles = 32,
        ModificarOrdenesYDetalles = 64,
        VerReportesAntiguos = 128,
        GestionarUsuarios = 256,
        Todos = 511
        

    }
}