// Models/Paciente.cs

namespace BioLabProject.Models;
public class PacienteModel {
    /*
     * el modelo de paciente es bastante sencillo
     */
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido {get; set;} = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Telefono {get; set;} = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

}