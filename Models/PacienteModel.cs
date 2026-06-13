
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioLabApi.Models;
public class PacienteModel : Auditable
{
    /*
     * el modelo de paciente es bastante sencillo
     */
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(20)]
    public string Nombre { get; set; } = string.Empty;
    [Required]
    [MaxLength(20)]
    public string Apellido {get; set;} = string.Empty;
    [Required]
    [MaxLength(10)]
    public string Cedula { get; set; } = string.Empty;

    [Required]
    public DateTime FechaNacimiento { get; set; }

    [Required]
    public string Sexo { get; set; } = "N/A";

    [MaxLength(15)] 
    public string Telefono {get; set;} = "N/A";
    [ MaxLength(100)]
    public string Direccion { get; set; } = "N/A";
    public bool IsActive { get; set; } = true;

    //estos campos son para cuando el paciente es un niño o una niña

    [MaxLength(50)]
    public string NombreAcompañante { get; set; } = "N/A";

    [MaxLength(10)]
    public string CedulaAcompañante { get; set; } = "N/A";

}